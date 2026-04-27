using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record InvoiceApprovalCompensationScheduledEvidence(
    InvoiceApprovalStatus PreviousState,
    InvoiceApprovalStatus CurrentState,
    string? CorrelationId,
    DateTimeOffset ObservedAt)
    : BusinessTransitionEvidence<InvoiceApprovalStatus>(
        ModelName: nameof(InvoiceApproval),
        TransitionName: "ScheduleApprovalCompensation",
        PreviousState: PreviousState,
        CurrentState: CurrentState,
        CorrelationId: CorrelationId,
        ObservedAt: ObservedAt);
