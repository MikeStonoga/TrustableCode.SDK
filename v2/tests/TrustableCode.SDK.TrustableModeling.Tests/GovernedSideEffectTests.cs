using TrustableCode.SDK.Samples.Ordering.SideEffects;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class GovernedSideEffectTests
{
    [Fact]
    public void Notify_fulfillment_should_execute_once_and_emit_side_effect_evidence()
    {
        var ledger = new InMemoryIdempotencyLedger();
        var notifications = new List<FulfillmentNotification>();
        var sideEffect = new NotifyFulfillmentSideEffect(ledger, notifications);

        var result = sideEffect.Execute(new FulfillmentNotification("order-1", "corr-side-effect-1"));

        Assert.Equal(SideEffectExecutionStatus.Executed, result.Status);
        Assert.Single(notifications);
        Assert.Equal("NotifyFulfillmentEvidence", result.Evidence.Name);
        Assert.Equal("corr-side-effect-1", result.Evidence.CorrelationId);
        Assert.Equal("Executed", result.Evidence.Metadata["side_effect.status"]);
    }

    [Fact]
    public void Notify_fulfillment_should_not_execute_twice_for_the_same_idempotency_key()
    {
        var ledger = new InMemoryIdempotencyLedger();
        var notifications = new List<FulfillmentNotification>();
        var sideEffect = new NotifyFulfillmentSideEffect(ledger, notifications);

        sideEffect.Execute(new FulfillmentNotification("order-1", "corr-side-effect-1"));
        var repeated = sideEffect.Execute(new FulfillmentNotification("order-1", "corr-side-effect-2"));

        Assert.Equal(SideEffectExecutionStatus.AlreadyApplied, repeated.Status);
        Assert.Single(notifications);
        Assert.Equal("AlreadyApplied", repeated.Evidence.Metadata["side_effect.status"]);
    }
}

