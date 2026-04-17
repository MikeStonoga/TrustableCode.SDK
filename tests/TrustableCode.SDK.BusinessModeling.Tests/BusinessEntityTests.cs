using TrustableCode.SDK.BusinessModeling.Events;
using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessEntityTests
{
    [Fact]
    public void DequeueBusinessEvents_returns_and_clears_recorded_business_events()
    {
        var model = ShippingOrder.Prepare();

        var firstDrain = model.DequeueBusinessEvents();
        var secondDrain = model.DequeueBusinessEvents();

        Assert.Single(firstDrain);
        Assert.Empty(secondDrain);
        Assert.IsType<OrderPreparedForShipping>(firstDrain[0]);
    }

    [Fact]
    public void EnsureAll_throws_aggregated_exception_when_multiple_rules_are_broken()
    {
        var exception = Assert.Throws<AggregatedBusinessRuleViolationException>(() =>
            DraftInvoice.Create(issueDateIsInPast: true, amountIsNegative: true));

        Assert.Equal(2, exception.Violations.Count);
        Assert.Contains("Issue date cannot be in the past.", exception.Violations);
        Assert.Contains("Invoice amount must be positive.", exception.Violations);
    }

    private sealed class ShippingOrder : BusinessEntity
    {
        public static ShippingOrder Prepare()
        {
            var order = new ShippingOrder();
            order.RecordBusinessEvent(new OrderPreparedForShipping(DateTimeOffset.UtcNow));
            return order;
        }
    }

    private sealed record OrderPreparedForShipping(DateTimeOffset OccurredAt) : BusinessEvent(OccurredAt);

    private sealed class DraftInvoice : BusinessEntity
    {
        public static DraftInvoice Create(bool issueDateIsInPast, bool amountIsNegative)
        {
            EnsureAll(notification => notification
                .Collect(new IssueDateMustNotBeInThePastRule(issueDateIsInPast))
                .Collect(new InvoiceAmountMustBePositiveRule(amountIsNegative)));

            return new DraftInvoice();
        }
    }

    private sealed class IssueDateMustNotBeInThePastRule(bool isBroken) : IBusinessInvariantRule
    {
        public string Description => "Issue date must not be in the past.";

        public void EnsureIsPreserved()
        {
            if (isBroken)
            {
                throw new BusinessRuleViolationException("Issue date cannot be in the past.");
            }
        }
    }

    private sealed class InvoiceAmountMustBePositiveRule(bool isBroken) : IBusinessInvariantRule
    {
        public string Description => "Invoice amount must be positive.";

        public void EnsureIsPreserved()
        {
            if (isBroken)
            {
                throw new BusinessRuleViolationException("Invoice amount must be positive.");
            }
        }
    }
}
