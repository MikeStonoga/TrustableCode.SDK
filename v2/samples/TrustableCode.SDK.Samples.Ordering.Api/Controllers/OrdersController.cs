using Microsoft.AspNetCore.Mvc;
using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Api.Models;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;

namespace TrustableCode.SDK.Samples.Ordering.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Tags("Orders")]
public sealed class OrdersController(
    IOrderSnapshotStore orders,
    IOrderingUnitOfWork unitOfWork,
    PersistedOrderingApplicationService persistedApplication) : ControllerBase
{
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OrderResponse> Get(string orderId)
    {
        var snapshot = orders.Find(orderId);

        return snapshot is null
            ? NotFound()
            : Ok(OrderResponse.From(snapshot));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<OperationResponse> Create(ExternalCreateOrderRequest request)
    {
        var result = persistedApplication.CreateOrder(request);
        unitOfWork.Commit();

        if (!result.Succeeded)
        {
            return BadRequest(OperationResponse.From(result));
        }

        return CreatedAtAction(
            nameof(Get),
            new { orderId = result.Order!.OrderId },
            OperationResponse.From(result));
    }

    [HttpPost("{orderId}/capture-payment")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> CapturePayment(
        string orderId,
        ExternalCapturePaymentRequest request)
        => Execute(() => persistedApplication.CapturePayment(orderId, request));

    [HttpPost("{orderId}/prepare-for-shipping")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> PrepareForShipping(
        string orderId,
        ExternalPrepareOrderForShippingRequest request)
        => Execute(() => persistedApplication.PrepareForShipping(orderId, request));

    [HttpPost("{orderId}/ship")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> Ship(
        string orderId,
        ExternalShipOrderRequest request)
        => Execute(() => persistedApplication.Ship(orderId, request));

    [HttpPost("{orderId}/deliver")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> Deliver(
        string orderId,
        ExternalDeliverOrderRequest request)
        => Execute(() => persistedApplication.Deliver(orderId, request));

    [HttpPost("{orderId}/cancel")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> Cancel(
        string orderId,
        ExternalCancelOrderRequest request)
        => Execute(() => persistedApplication.Cancel(orderId, request));

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
}
