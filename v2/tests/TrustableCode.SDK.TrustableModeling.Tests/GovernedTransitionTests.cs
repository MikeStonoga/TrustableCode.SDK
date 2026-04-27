using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class GovernedTransitionTests
{
    [Fact]
    public void Order_should_move_through_the_complete_happy_path()
    {
        var created = OrderFactory.Create(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 2)],
            RequestedStatus: null,
            CorrelationId: "corr-create-1"));

        var order = created.Value!;

        order.CapturePayment(new CapturePaymentRequirement(
            PaymentCaptured: true,
            PaymentReference: "pay-1",
            CorrelationId: "corr-payment-1"));
        order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            CorrelationId: "corr-prepare-1"));
        order.Ship(new ShipOrderRequirement(
            Carrier: "DHL",
            TrackingCode: "track-1",
            CorrelationId: "corr-ship-1"));
        var delivered = order.Deliver(new DeliverOrderRequirement(
            ProofOfDeliveryCaptured: true,
            DeliveredAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-deliver-1"));

        Assert.True(created.WasAccepted);
        Assert.Equal(TransitionExecutionStatus.Applied, delivered.Status);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(
            [
                "OrderCreated",
                "OrderPaymentCaptured",
                "OrderPreparedForShipping",
                "OrderShipped",
                "OrderDelivered"
            ],
            order.Events);
        Assert.Contains("OrderCreatedEvidence", order.Evidence);
        Assert.Contains("OrderDeliveredEvidence", order.Evidence);
    }

    [Fact]
    public void Capture_payment_should_move_order_to_paid_awaiting_fulfillment()
    {
        var order = Order.Rehydrate(OrderStatus.AwaitingPayment);

        var result = order.CapturePayment(new CapturePaymentRequirement(
            PaymentCaptured: true,
            PaymentReference: "pay-1",
            CorrelationId: "corr-payment-1"));

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, order.Status);
        Assert.Contains("OrderPaymentCaptured", order.Events);
        Assert.Contains("OrderPaymentCapturedEvidence", order.Evidence);
    }

    [Fact]
    public void Prepare_for_shipping_should_apply_state_and_record_declared_outputs()
    {
        var order = Order.Rehydrate(OrderStatus.PaidAwaitingFulfillment);

        var result = order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            CorrelationId: "corr-1"));

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.True(result.WasApplied);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, result.PreviousState);
        Assert.Equal(OrderStatus.ReadyForShipping, result.CurrentState);
        Assert.Equal(OrderStatus.ReadyForShipping, order.Status);
        Assert.Contains("OrderPreparedForShipping", order.Events);
        Assert.Contains("OrderPreparedForShippingEvidence", order.Evidence);
    }

    [Fact]
    public void Prepare_for_shipping_should_reject_when_preconditions_are_not_satisfied()
    {
        var order = Order.Rehydrate(OrderStatus.PaidAwaitingFulfillment);

        var result = order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: false,
            StockReserved: false,
            CorrelationId: "corr-2"));

        Assert.Equal(TransitionExecutionStatus.Rejected, result.Status);
        Assert.False(result.WasApplied);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, order.Status);
        Assert.Equal(2, result.RejectionReasons.Count);
        Assert.Contains("Payment must be captured before the order can be prepared for shipping.", result.RejectionReasons);
        Assert.Contains("Stock must be reserved before the order can be prepared for shipping.", result.RejectionReasons);
        Assert.Contains("OrderPreparationRejectedEvidence", order.Evidence);
        Assert.Equal(2, result.RejectionEvidence.Count);
        Assert.Equal(2, order.BusinessEvidence.Count);
        Assert.Contains(order.BusinessEvidence, evidence => evidence.Name == "PaymentCapturedBeforeShipmentPreparationViolation");
        Assert.Contains(order.BusinessEvidence, evidence => evidence.Name == "StockReservedBeforeShipmentPreparationViolation");
        Assert.Empty(order.Events);
    }

    [Fact]
    public void Prepare_for_shipping_should_be_idempotent_when_already_ready_for_shipping()
    {
        var order = Order.Rehydrate(OrderStatus.ReadyForShipping);

        var result = order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            CorrelationId: "corr-3"));

        Assert.Equal(TransitionExecutionStatus.AlreadyApplied, result.Status);
        Assert.True(result.WasApplied);
        Assert.Equal(OrderStatus.ReadyForShipping, result.CurrentState);
        Assert.Empty(order.Events);
    }

    [Fact]
    public void Cancel_should_reject_after_order_is_shipped()
    {
        var order = Order.Rehydrate(OrderStatus.Shipped);

        var result = order.Cancel(new CancelOrderRequirement(
            Reason: "Customer changed their mind.",
            CorrelationId: "corr-cancel-1"));

        Assert.Equal(TransitionExecutionStatus.Rejected, result.Status);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Contains("Shipped or delivered orders cannot be cancelled by this transition.", result.RejectionReasons);
        Assert.Contains("OrderCancellationRejectedEvidence", order.Evidence);
    }
}
