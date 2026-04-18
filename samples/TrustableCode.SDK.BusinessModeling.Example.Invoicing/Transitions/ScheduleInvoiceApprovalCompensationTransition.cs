using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing.Transitions;

public sealed class ScheduleInvoiceApprovalCompensationTransition(
    Func<InvoiceApprovalStatus> currentStateAccessor,
    Action<InvoiceApprovalStatus> apply)
    : NamedBusinessTransition<InvoiceApprovalStatus>(
        name: "ScheduleApprovalCompensation",
        to: InvoiceApprovalStatus.ApprovalCompensationPending,
        currentStateAccessor: currentStateAccessor,
        apply: apply)
{
    protected override bool CanTransitionFrom(InvoiceApprovalStatus currentState)
        => currentState == InvoiceApprovalStatus.Approved;

    protected override BusinessRuleViolationException CreateException(InvoiceApprovalStatus currentState)
        => new("Only approved invoices may schedule approval compensation.");
}
