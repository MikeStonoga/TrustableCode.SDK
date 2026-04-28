using Microsoft.AspNetCore.Mvc;
using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Api.Models;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;

namespace TrustableCode.SDK.Samples.Ordering.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    IOrderSnapshotStore orders,
    IOrderingOutbox outbox,
    IOrderingUnitOfWork unitOfWork,
    OrderingApplicationService application,
    PersistedOrderingApplicationService persistedApplication) : ControllerBase
{
    [HttpGet("{orderId}")]
    public ActionResult<OrderResponse> Get(string orderId)
    {
        var snapshot = orders.Find(orderId);

        return snapshot is null
            ? NotFound()
            : Ok(OrderResponse.From(snapshot));
    }

    [HttpPost]
    public ActionResult<OperationResponse> Create(ExternalCreateOrderRequest request)
    {
        var result = application.CreateOrder(request);
        if (!result.Succeeded)
        {
            unitOfWork.Commit();
            return BadRequest(OperationResponse.From(result));
        }

        var order = result.Order!;
        orders.Save(OrderPersistenceSnapshot.From(order));
        EnqueueProducedEvents(order.OrderId, request.CorrelationId, result.ProducedEvents);
        unitOfWork.Commit();

        return CreatedAtAction(
            nameof(Get),
            new { orderId = order.OrderId },
            OperationResponse.From(result));
    }

    [HttpPost("{orderId}/capture-payment")]
    public ActionResult<OperationResponse> CapturePayment(
        string orderId,
        ExternalCapturePaymentRequest request)
        => ExecutePersisted(orderId, request.CorrelationId, order =>
            application.CapturePayment(order, request));

    [HttpPost("{orderId}/prepare-for-shipping")]
    public ActionResult<OperationResponse> PrepareForShipping(
        string orderId,
        ExternalPrepareOrderForShippingRequest request)
        => Execute(() => persistedApplication.PrepareForShipping(orderId, request));

    [HttpPost("{orderId}/ship")]
    public ActionResult<OperationResponse> Ship(
        string orderId,
        ExternalShipOrderRequest request)
        => Execute(() => persistedApplication.Ship(orderId, request));

    [HttpPost("{orderId}/deliver")]
    public ActionResult<OperationResponse> Deliver(
        string orderId,
        ExternalDeliverOrderRequest request)
        => ExecutePersisted(orderId, request.CorrelationId, order =>
            application.Deliver(order, request));

    [HttpPost("{orderId}/cancel")]
    public ActionResult<OperationResponse> Cancel(
        string orderId,
        ExternalCancelOrderRequest request)
        => ExecutePersisted(orderId, request.CorrelationId, order =>
            application.Cancel(order, request));

    private ActionResult<OperationResponse> ExecutePersisted(
        string orderId,
        string correlationId,
        Func<Order, OrderingApplicationResult> execute)
        => Execute(() =>
        {
            var snapshot = orders.Find(orderId)
                ?? throw new InvalidOperationException($"Order '{orderId}' was not found in persistence.");

            var order = Order.Rehydrate(snapshot);
            var result = execute(order);
            if (!result.Succeeded)
            {
                return result;
            }

            orders.Save(OrderPersistenceSnapshot.From(order));
            EnqueueProducedEvents(order.OrderId, correlationId, result.ProducedEvents);

            return result;
        });

    private ActionResult<OperationResponse> Execute(Func<OrderingApplicationResult> execute)
    {
        try
        {
            var result = execute();
            unitOfWork.Commit();
            if (!result.Succeeded)
            {
                return result.WasAccepted
                    ? Conflict(OperationResponse.From(result))
                    : BadRequest(OperationResponse.From(result));
            }

            return Ok(OperationResponse.From(result));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { error = exception.Message });
        }
    }

    private void EnqueueProducedEvents(
        string orderId,
        string correlationId,
        IEnumerable<string> producedEvents)
    {
        foreach (var eventName in producedEvents)
        {
            outbox.Enqueue(new OrderingOutboxMessage(
                StreamName: $"Order-{orderId}",
                EventName: eventName,
                OrderId: orderId,
                CorrelationId: correlationId));
        }
    }
}
