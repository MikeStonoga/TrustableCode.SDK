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
    private readonly IOrderingUnitOfWork _unitOfWork;
    private readonly OrderingApplicationService _application;

    public PersistedOrderingApplicationService(
        IOrderSnapshotStore orders,
        IOrderingOutbox outbox,
        IOrderingUnitOfWork unitOfWork,
        IBusinessEvidenceSink evidenceSink,
        ISideEffectLifecycleStore sideEffectLifecycleStore)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _application = new OrderingApplicationService(evidenceSink, sideEffectLifecycleStore);
    }

    public OrderingApplicationResult CreateOrder(ExternalCreateOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = _application.CreateOrder(request);
        if (!result.Succeeded)
        {
            _unitOfWork.Commit();
            return result;
        }

        var order = result.Order!;
        _orders.Save(OrderPersistenceSnapshot.From(order));
        EnqueueProducedEvents(order.OrderId, request.CorrelationId, result.ProducedEvents);
        _unitOfWork.Commit();

        return result;
    }

    public OrderingApplicationResult CapturePayment(
        string orderId,
        ExternalCapturePaymentRequest request)
        => ExecutePersisted(
            orderId,
            request.CorrelationId,
            order => _application.CapturePayment(order, request));

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

    public OrderingApplicationResult Deliver(
        string orderId,
        ExternalDeliverOrderRequest request)
        => ExecutePersisted(
            orderId,
            request.CorrelationId,
            order => _application.Deliver(order, request));

    public OrderingApplicationResult Cancel(
        string orderId,
        ExternalCancelOrderRequest request)
        => ExecutePersisted(
            orderId,
            request.CorrelationId,
            order => _application.Cancel(order, request));

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
            _unitOfWork.Commit();
            return result;
        }

        _orders.Save(OrderPersistenceSnapshot.From(order));
        EnqueueProducedEvents(order.OrderId, correlationId, result.ProducedEvents);
        _unitOfWork.Commit();

        return result;
    }

    private void EnqueueProducedEvents(
        string orderId,
        string correlationId,
        IEnumerable<string> producedEvents)
    {
        foreach (var eventName in producedEvents)
        {
            _outbox.Enqueue(new OrderingOutboxMessage(
                StreamName: $"Order-{orderId}",
                EventName: eventName,
                OrderId: orderId,
                CorrelationId: correlationId));
        }
    }
}
