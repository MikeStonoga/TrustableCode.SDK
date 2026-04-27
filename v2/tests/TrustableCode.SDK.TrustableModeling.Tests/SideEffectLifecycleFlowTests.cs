using TrustableCode.SDK.Samples.Ordering.SideEffects;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class SideEffectLifecycleFlowTests
{
    [Fact]
    public void Plan_persist_and_publish_should_advance_lifecycle_and_record_each_evidence()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var recorder = new BusinessEvidenceRecorder(sink);
        var lifecycle = new GovernedSideEffectLifecycle<FulfillmentNotification>(
            name: "NotifyFulfillment",
            idempotencyKey: notification => $"NotifyFulfillment:{notification.OrderId}",
            store: new InMemorySideEffectLifecycleStore());

        var published = lifecycle.PlanPersistAndPublish(
            new FulfillmentNotification("order-1", "corr-lifecycle-flow-1"),
            recorder,
            correlationId: "corr-lifecycle-flow-1");

        Assert.Equal(SideEffectLifecycleStatus.Published, published.Status);
        Assert.Equal(3, published.History.Count);
        Assert.Equal(
            [
                "NotifyFulfillmentPlannedEvidence",
                "NotifyFulfillmentPersistedEvidence",
                "NotifyFulfillmentPublishedEvidence"
            ],
            sink.Evidence.Select(evidence => evidence.Name));
    }
}
