using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class BusinessInvariantTests
{
    [Fact]
    public void Invariant_set_should_report_only_violated_business_truths()
    {
        var context = new TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>(
            TransitionName: "PrepareForShipping",
            CurrentState: OrderStatus.Paid,
            TargetState: OrderStatus.ReadyForShipping,
            Input: new PrepareOrderForShippingRequirement(
                PaymentCaptured: true,
                StockReserved: false,
                CorrelationId: "corr-invariant-1"));

        var violations = new InvariantSet<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>>(
            OrderFulfillmentInvariants.PrepareForShipping)
            .FindViolations(context);

        Assert.Single(violations);
        Assert.Equal("StockReservedBeforeShipmentPreparation", violations[0].Code);
        Assert.Equal("Stock must be reserved before the order can be prepared for shipping.", violations[0].Message);
    }
}

