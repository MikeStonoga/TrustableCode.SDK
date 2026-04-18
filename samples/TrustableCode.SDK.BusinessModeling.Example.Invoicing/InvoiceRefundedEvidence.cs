using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record InvoiceRefundedEvidence(
    InvoiceStatus PreviousState,
    InvoiceStatus CurrentState,
    string? CorrelationId,
    DateTimeOffset ObservedAt)
    : BusinessTransitionEvidence<InvoiceStatus>(
        ModelName: nameof(Invoice),
        TransitionName: "Refund",
        PreviousState: PreviousState,
        CurrentState: CurrentState,
        CorrelationId: CorrelationId,
        ObservedAt: ObservedAt);
