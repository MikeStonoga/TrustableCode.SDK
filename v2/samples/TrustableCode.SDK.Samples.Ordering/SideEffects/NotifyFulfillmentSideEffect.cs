using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.Samples.Ordering.SideEffects;

public sealed class NotifyFulfillmentSideEffect
{
    private readonly GovernedSideEffect<FulfillmentNotification> _sideEffect;

    public NotifyFulfillmentSideEffect(IIdempotencyLedger ledger, ICollection<FulfillmentNotification> notifications)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        _sideEffect = new GovernedSideEffect<FulfillmentNotification>(
            name: "NotifyFulfillment",
            idempotencyKey: notification => $"NotifyFulfillment:{notification.OrderId}",
            execute: notifications.Add,
            ledger: ledger);
    }

    public SideEffectExecutionResult Execute(FulfillmentNotification notification)
        => _sideEffect.Execute(notification, notification.CorrelationId);
}

