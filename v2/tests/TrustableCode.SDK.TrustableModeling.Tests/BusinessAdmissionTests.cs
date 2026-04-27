using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class BusinessAdmissionTests
{
    [Fact]
    public void Order_factory_should_create_order_awaiting_payment_after_creation_admission()
    {
        var result = OrderFactory.Create(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: null,
            CorrelationId: "corr-create-1"));

        Assert.True(result.WasAccepted);
        Assert.NotNull(result.Value);
        Assert.Equal(OrderStatus.PlacedAwaitingPayment, result.Value.Status);
        Assert.Equal("order-1", result.Value.OrderId);
        Assert.Contains("OrderCreated", result.Value.Events);
        Assert.Contains("OrderCreatedEvidence", result.Value.Evidence);
    }

    [Fact]
    public void Order_factory_should_reject_arbitrary_initial_status()
    {
        var result = OrderFactory.Create(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: "PaidAwaitingFulfillment",
            CorrelationId: "corr-create-2"));

        Assert.False(result.WasAccepted);
        Assert.Null(result.Value);
        Assert.Contains(
            "External callers may create an order, but may not submit an arbitrary initial order status.",
            result.RejectionReasons);
        Assert.Single(result.RejectionEvidence);
        Assert.Equal("OrderCreationRejectedEvidence", result.RejectionEvidence[0].Name);
    }

    [Fact]
    public void Prepare_order_for_shipping_admission_should_accept_intent_without_target_status()
    {
        var admission = PrepareOrderForShippingAdmission.Create();

        var result = admission.Admit(new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "",
            CorrelationId: "corr-admission-1"));

        Assert.True(result.WasAccepted);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.PaymentCaptured);
        Assert.True(result.Value.StockReserved);
        Assert.Equal("corr-admission-1", result.Value.CorrelationId);
    }

    [Fact]
    public void Prepare_order_for_shipping_admission_should_reject_arbitrary_status_mutation()
    {
        var admission = PrepareOrderForShippingAdmission.Create();

        var result = admission.Admit(new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "ShippedWaitingDelivery",
            CorrelationId: "corr-admission-2"));

        Assert.False(result.WasAccepted);
        Assert.Null(result.Value);
        Assert.Contains(
            "External callers may request shipment preparation, but may not submit an arbitrary target order status.",
            result.RejectionReasons);
        Assert.Single(result.RejectionEvidence);
        Assert.Equal("BoundaryMustReceiveIntentNotStatusRejected", result.RejectionEvidence[0].Name);
        Assert.Equal("PrepareOrderForShippingAdmission", result.RejectionEvidence[0].Metadata["admission.name"]);
    }

    [Fact]
    public void Prepare_order_for_shipping_admission_should_reject_missing_correlation()
    {
        var admission = PrepareOrderForShippingAdmission.Create();

        var result = admission.Admit(new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "",
            CorrelationId: ""));

        Assert.False(result.WasAccepted);
        Assert.Contains("A correlation id is required before order preparation can be admitted.", result.RejectionReasons);
        Assert.Single(result.RejectionEvidence);
        Assert.Equal("CorrelationIdRequiredRejected", result.RejectionEvidence[0].Name);
    }

    [Fact]
    public void Capture_payment_admission_should_accept_payment_capture_facts()
    {
        var admission = CapturePaymentAdmission.Create();

        var result = admission.Admit(new ExternalCapturePaymentRequest(
            PaymentCaptured: true,
            PaymentReference: "pay-1",
            RequestedStatus: null,
            CorrelationId: "corr-payment-1"));

        Assert.True(result.WasAccepted);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.PaymentCaptured);
        Assert.Equal("pay-1", result.Value.PaymentReference);
    }

    [Fact]
    public void Ship_order_admission_should_reject_arbitrary_status_mutation()
    {
        var admission = ShipOrderAdmission.Create();

        var result = admission.Admit(new ExternalShipOrderRequest(
            Carrier: "DHL",
            TrackingCode: "track-1",
            RequestedStatus: "ShippedWaitingDelivery",
            CorrelationId: "corr-ship-1"));

        Assert.False(result.WasAccepted);
        Assert.Equal("OrderShipmentRejectedEvidence", result.RejectionEvidence[0].Name);
        Assert.Contains(
            "External callers may confirm shipment, but may not submit an arbitrary target order status.",
            result.RejectionReasons);
    }

    [Fact]
    public void Deliver_order_admission_should_accept_delivery_evidence()
    {
        var deliveredAt = DateTimeOffset.UtcNow;
        var admission = DeliverOrderAdmission.Create();

        var result = admission.Admit(new ExternalDeliverOrderRequest(
            ProofOfDeliveryCaptured: true,
            DeliveredAt: deliveredAt,
            RequestedStatus: null,
            CorrelationId: "corr-delivery-1"));

        Assert.True(result.WasAccepted);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.ProofOfDeliveryCaptured);
        Assert.Equal(deliveredAt, result.Value.DeliveredAt);
    }

    [Fact]
    public void Cancel_order_admission_should_reject_missing_correlation()
    {
        var admission = CancelOrderAdmission.Create();

        var result = admission.Admit(new ExternalCancelOrderRequest(
            Reason: "Customer requested cancellation.",
            RequestedStatus: null,
            CorrelationId: ""));

        Assert.False(result.WasAccepted);
        Assert.Equal("OrderCancellationRejectedEvidence", result.RejectionEvidence[0].Name);
        Assert.Contains("A correlation id is required before cancellation can be admitted.", result.RejectionReasons);
    }
}
