using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing.Exceptions;
using TrustableCode.SDK.BusinessModeling.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing.Transitions;

public sealed class RefundInvoiceTransition(
    InvoiceStatus targetStatus,
    Func<InvoiceStatus> currentStateAccessor,
    Action<InvoiceStatus> apply)
    : NamedBusinessTransition<InvoiceStatus>(
        name: "Refund",
        to: targetStatus,
        currentStateAccessor: currentStateAccessor,
        apply: apply)
{
    protected override bool CanTransitionFrom(InvoiceStatus currentState)
        => currentState is InvoiceStatus.Captured or InvoiceStatus.PartiallyRefunded;

    protected override BusinessRuleViolationException CreateException(InvoiceStatus currentState)
        => new RefundRequiresCapturedInvoiceException();
}
