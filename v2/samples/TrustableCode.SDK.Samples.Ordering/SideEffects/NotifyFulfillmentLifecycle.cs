using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.Samples.Ordering.SideEffects;

public sealed class NotifyFulfillmentLifecycle
{
    private readonly GovernedSideEffectLifecycle<FulfillmentNotification> _lifecycle;

    public NotifyFulfillmentLifecycle(ISideEffectLifecycleStore store)
    {
        _lifecycle = new GovernedSideEffectLifecycle<FulfillmentNotification>(
            name: "NotifyFulfillment",
            idempotencyKey: notification => $"NotifyFulfillment:{notification.OrderId}",
            store: store);
    }

    public SideEffectLifecycleRecord Plan(FulfillmentNotification notification)
        => _lifecycle.Plan(notification, notification.CorrelationId);

    public SideEffectLifecycleRecord Persist(FulfillmentNotification notification)
        => _lifecycle.Persist(IdempotencyKey(notification), notification.CorrelationId);

    public SideEffectLifecycleRecord Publish(FulfillmentNotification notification)
        => _lifecycle.Publish(IdempotencyKey(notification), notification.CorrelationId);

    public SideEffectLifecycleRecord Confirm(FulfillmentNotification notification)
        => _lifecycle.Confirm(IdempotencyKey(notification), notification.CorrelationId);

    public SideEffectLifecycleRecord RequireCompensation(FulfillmentNotification notification, string reason)
        => _lifecycle.RequireCompensation(IdempotencyKey(notification), reason, notification.CorrelationId);

    public SideEffectLifecycleRecord Compensate(FulfillmentNotification notification)
        => _lifecycle.Compensate(IdempotencyKey(notification), notification.CorrelationId);

    public SideEffectLifecycleRecord PlanPersistAndPublish(
        FulfillmentNotification notification,
        BusinessEvidenceRecorder recorder)
        => _lifecycle.PlanPersistAndPublish(notification, recorder, notification.CorrelationId);

    private static string IdempotencyKey(FulfillmentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return $"NotifyFulfillment:{notification.OrderId}";
    }
}
