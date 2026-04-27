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

    [Fact]
    public void Notify_fulfillment_lifecycle_should_track_planned_persisted_published_and_confirmed_steps()
    {
        var store = new InMemorySideEffectLifecycleStore();
        var lifecycle = new NotifyFulfillmentLifecycle(store);
        var notification = new FulfillmentNotification("order-1", "corr-lifecycle-1");

        var planned = lifecycle.Plan(notification);
        var persisted = lifecycle.Persist(notification);
        var published = lifecycle.Publish(notification);
        var confirmed = lifecycle.Confirm(notification);

        Assert.Equal(SideEffectLifecycleStatus.Planned, planned.Status);
        Assert.Equal(SideEffectLifecycleStatus.Persisted, persisted.Status);
        Assert.Equal(SideEffectLifecycleStatus.Published, published.Status);
        Assert.Equal(SideEffectLifecycleStatus.Confirmed, confirmed.Status);
        Assert.Equal("NotifyFulfillment:order-1", confirmed.IdempotencyKey);
        Assert.Equal("corr-lifecycle-1", confirmed.Evidence.CorrelationId);
        Assert.Equal("Confirmed", confirmed.Evidence.Metadata["side_effect.lifecycle_status"]);
        Assert.Equal(4, confirmed.History.Count);
    }

    [Fact]
    public void Notify_fulfillment_lifecycle_should_reuse_existing_plan_for_the_same_idempotency_key()
    {
        var store = new InMemorySideEffectLifecycleStore();
        var lifecycle = new NotifyFulfillmentLifecycle(store);

        var first = lifecycle.Plan(new FulfillmentNotification("order-1", "corr-lifecycle-1"));
        var repeated = lifecycle.Plan(new FulfillmentNotification("order-1", "corr-lifecycle-2"));

        Assert.Same(first, repeated);
        Assert.Single(store.All());
        Assert.Equal("corr-lifecycle-1", repeated.Evidence.CorrelationId);
    }

    [Fact]
    public void Notify_fulfillment_lifecycle_should_track_compensation()
    {
        var store = new InMemorySideEffectLifecycleStore();
        var lifecycle = new NotifyFulfillmentLifecycle(store);
        var notification = new FulfillmentNotification("order-1", "corr-lifecycle-1");

        lifecycle.Plan(notification);
        lifecycle.Persist(notification);
        lifecycle.Publish(notification);
        var compensationRequired = lifecycle.RequireCompensation(notification, "Carrier rejected the shipment.");
        var compensated = lifecycle.Compensate(notification);

        Assert.Equal(SideEffectLifecycleStatus.CompensationRequired, compensationRequired.Status);
        Assert.Equal(
            "Carrier rejected the shipment.",
            compensationRequired.Evidence.Metadata["side_effect.compensation_reason"]);
        Assert.Equal(SideEffectLifecycleStatus.Compensated, compensated.Status);
        Assert.Equal(5, compensated.History.Count);
    }
}
