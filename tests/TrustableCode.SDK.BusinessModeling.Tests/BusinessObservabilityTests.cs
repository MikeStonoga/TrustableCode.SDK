using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessObservabilityTests
{
    [Fact]
    public void BusinessTransitionEvidence_captures_meaningful_transition_context()
    {
        var observedAt = new DateTimeOffset(2026, 4, 17, 20, 0, 0, TimeSpan.Zero);
        var evidence = new BusinessTransitionEvidence<OrderStatus>(
            ModelName: "Order",
            TransitionName: "PrepareForShipping",
            PreviousState: OrderStatus.Paid,
            CurrentState: OrderStatus.ReadyForShipping,
            CorrelationId: "corr-123",
            ObservedAt: observedAt);

        Assert.Equal("business-transition", evidence.EvidenceType);
        Assert.Equal(OrderStatus.Paid, evidence.PreviousState);
        Assert.Equal(OrderStatus.ReadyForShipping, evidence.CurrentState);
        Assert.Equal("PrepareForShipping", evidence.TransitionName);
        Assert.Equal("corr-123", evidence.CorrelationId);
    }

    [Fact]
    public void InvariantViolationEvidence_exposes_business_context()
    {
        var evidence = new InvariantViolationEvidence(
            ModelName: "Refund",
            InvariantName: "RefundMustNotExceedCapturedAmount",
            Message: "Refund amount exceeded the captured amount.",
            CorrelationId: "refund-42",
            ObservedAt: DateTimeOffset.UtcNow);

        Assert.Equal("invariant-violation", evidence.EvidenceType);
        Assert.Equal("Refund", evidence.ModelName);
        Assert.Equal("RefundMustNotExceedCapturedAmount", evidence.InvariantName);
    }

    private enum OrderStatus
    {
        Paid = 1,
        ReadyForShipping = 2
    }
}
