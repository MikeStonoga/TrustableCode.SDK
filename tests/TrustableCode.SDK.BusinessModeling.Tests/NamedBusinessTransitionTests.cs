using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class NamedBusinessTransitionTests
{
    [Fact]
    public void Execute_applies_transition_and_returns_named_direction()
    {
        var invoice = new InvoiceModel();
        var transition = invoice.PutOnHold();

        Assert.Equal(InvoiceStatus.Pending, transition.From);
        Assert.Equal(InvoiceStatus.OnHold, transition.To);
        Assert.Equal("PutOnHold", transition.Name);
        Assert.Equal(InvoiceStatus.OnHold, invoice.Status);
    }

    [Fact]
    public void Execute_throws_when_transition_is_not_allowed()
    {
        var invoice = new InvoiceModel(InvoiceStatus.Processed);

        var exception = Assert.Throws<BusinessRuleViolationException>(() => invoice.PutOnHold());

        Assert.Equal("Invoice may only be put on hold from Pending or Error.", exception.Message);
    }

    private sealed class InvoiceModel(InvoiceStatus status = InvoiceStatus.Pending)
    {
        public InvoiceStatus Status { get; private set; } = status;

        public BusinessTransition<InvoiceStatus> PutOnHold()
        {
            var transition = new NamedBusinessTransition<InvoiceStatus>(
                name: "PutOnHold",
                to: InvoiceStatus.OnHold,
                currentStateAccessor: () => Status,
                apply: next => Status = next,
                canTransition: current => current is InvoiceStatus.Pending or InvoiceStatus.Error,
                exceptionFactory: _ => new BusinessRuleViolationException("Invoice may only be put on hold from Pending or Error."));

            return transition.Execute();
        }
    }

    private enum InvoiceStatus
    {
        Pending = 1,
        Error = 2,
        OnHold = 3,
        Processed = 4
    }
}
