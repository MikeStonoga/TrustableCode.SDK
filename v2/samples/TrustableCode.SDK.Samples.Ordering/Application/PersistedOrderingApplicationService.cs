using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.Samples.Ordering;

/// <summary>
/// Application-layer example that loads persisted state, executes governed behavior, and saves a new snapshot.
/// </summary>
public sealed class PersistedOrderingApplicationService
{
    private readonly IOrderSnapshotStore _orders;
    private readonly IOrderingOutbox _outbox;
    private readonly OrderingApplicationService _application;

    public PersistedOrderingApplicationService(
        IOrderSnapshotStore orders,
        IOrderingOutbox outbox,
        IBusinessEvidenceSink evidenceSink,
        ISideEffectLifecycleStore sideEffectLifecycleStore)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _application = new OrderingApplicationService(evidenceSink, sideEffectLifecycleStore);
    }

    public OrderingApplicationResult PrepareForShipping(
        string orderId,
        ExternalPrepareOrderForShippingRequest request)
        => ExecutePersisted(
            orderId,
            request.CorrelationId,
            order => _application.PrepareForShipping(order, request));

    public OrderingApplicationResult Ship(
        string orderId,
        ExternalShipOrderRequest request)
        => ExecutePersisted(
            orderId,
            request.CorrelationId,
            order => _application.Ship(order, request));

    private OrderingApplicationResult ExecutePersisted(
        string orderId,
        string correlationId,
        Func<Order, OrderingApplicationResult> execute)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation id is required.", nameof(correlationId));
        }

        ArgumentNullException.ThrowIfNull(execute);

        var snapshot = _orders.Find(orderId)
            ?? throw new InvalidOperationException($"Order '{orderId}' was not found in persistence.");

        var order = Order.Rehydrate(snapshot);
        var result = execute(order);

        if (!result.Succeeded)
        {
            return result;
        }

        _orders.Save(OrderPersistenceSnapshot.From(order));

        foreach (var eventName in result.ProducedEvents)
        {
            _outbox.Enqueue(new OrderingOutboxMessage(
                StreamName: $"Order-{order.OrderId}",
                EventName: eventName,
                OrderId: order.OrderId,
                CorrelationId: correlationId));
        }

        return result;
    }
}
