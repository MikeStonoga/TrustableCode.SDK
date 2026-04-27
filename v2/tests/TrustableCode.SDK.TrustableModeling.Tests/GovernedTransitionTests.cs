using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class GovernedTransitionTests
{
    [Fact]
    public void Order_should_move_through_the_complete_happy_path()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var created = OrderFactory.Create(scenario.CreateOrderRequest());

        var order = created.Value!;

        order.CapturePayment(scenario.CapturedPayment());
        order.PrepareForShipping(scenario.ShippingPreparation());
        order.Ship(scenario.Shipment());
        var delivered = order.Deliver(scenario.Delivery());

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
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.PlacedAwaitingPayment);

        var result = order.CapturePayment(scenario.CapturedPayment());

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, order.Status);
        Assert.Contains("OrderPaymentCaptured", order.Events);
        Assert.Contains("OrderPaymentCapturedEvidence", order.Evidence);
    }

    [Fact]
    public void Prepare_for_shipping_should_apply_state_and_record_declared_outputs()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.PaidAwaitingFulfillment);

        var result = order.PrepareForShipping(scenario.ShippingPreparation(correlationId: "corr-1"));

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.True(result.WasApplied);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, result.PreviousState);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, result.CurrentState);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, order.Status);
        Assert.Contains("OrderPreparedForShipping", order.Events);
        Assert.Contains("OrderPreparedForShippingEvidence", order.Evidence);
    }

    [Fact]
    public void Prepare_for_shipping_should_reject_when_preconditions_are_not_satisfied()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.PaidAwaitingFulfillment);

        var result = order.PrepareForShipping(scenario.ShippingPreparation(
            paymentCaptured: false,
            stockReserved: false,
            correlationId: "corr-2"));

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
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.FulfilledReadyForShipping);

        var result = order.PrepareForShipping(scenario.ShippingPreparation(correlationId: "corr-3"));

        Assert.Equal(TransitionExecutionStatus.AlreadyApplied, result.Status);
        Assert.True(result.WasApplied);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, result.CurrentState);
        Assert.Empty(order.Events);
    }

    [Fact]
    public void Cancel_should_reject_after_order_is_shipped()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.ShippedWaitingDelivery);

        var result = order.Cancel(scenario.Cancellation());

        Assert.Equal(TransitionExecutionStatus.Rejected, result.Status);
        Assert.Equal(OrderStatus.ShippedWaitingDelivery, order.Status);
        Assert.Contains("Orders waiting for delivery or already delivered cannot be cancelled by this transition.", result.RejectionReasons);
        Assert.Contains("OrderCancellationRejectedEvidence", order.Evidence);
    }
}
