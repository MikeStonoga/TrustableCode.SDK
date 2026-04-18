using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record OrderPreparedForShippingEvidence(
    OrderStatus PreviousState,
    OrderStatus CurrentState,
    string? CorrelationId,
    DateTimeOffset ObservedAt)
    : BusinessTransitionEvidence<OrderStatus>(
        ModelName: nameof(Order),
        TransitionName: "PrepareForShipping",
        PreviousState: PreviousState,
        CurrentState: CurrentState,
        CorrelationId: CorrelationId,
        ObservedAt: ObservedAt);
