using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record OrderCancelledEvidence(
    OrderStatus PreviousState,
    OrderStatus CurrentState,
    string? CorrelationId,
    DateTimeOffset ObservedAt)
    : BusinessTransitionEvidence<OrderStatus>(
        ModelName: nameof(Order),
        TransitionName: "Cancel",
        PreviousState: PreviousState,
        CurrentState: CurrentState,
        CorrelationId: CorrelationId,
        ObservedAt: ObservedAt);
