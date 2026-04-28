using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.TrustableModeling.Tests;

internal sealed class OrderingScenarioBuilder
{
    private readonly DateTimeOffset _deliveredAt = new(2026, 4, 27, 12, 0, 0, TimeSpan.Zero);

    public static OrderingScenarioBuilder Create()
        => new();

    public Order RehydratedOrder(OrderStatus status)
        => Order.Rehydrate(PersistedOrder(status));

    public OrderPersistenceSnapshot PersistedOrder(OrderStatus status)
        => new(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            Status: status);

    public Order CreatedOrder()
        => CreatedOrderResult().Value!;

    public ExternalCreateOrderRequest CreateOrderRequest(
        string? requestedStatus = null,
        string correlationId = "corr-create-1")
        => new(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: requestedStatus,
            CorrelationId: correlationId);

    public CapturePaymentRequirement CapturedPayment(
        bool paymentCaptured = true,
        string paymentReference = "pay-1",
        string correlationId = "corr-payment-1")
        => new(paymentCaptured, paymentReference, correlationId);

    public PrepareOrderForShippingRequirement ShippingPreparation(
        bool paymentCaptured = true,
        bool stockReserved = true,
        string correlationId = "corr-prepare-1")
        => new(paymentCaptured, stockReserved, correlationId);

    public ShipOrderRequirement Shipment(
        string carrier = "DHL",
        string trackingCode = "track-1",
        string correlationId = "corr-ship-1")
        => new(carrier, trackingCode, correlationId);

    public DeliverOrderRequirement Delivery(
        bool proofOfDeliveryCaptured = true,
        string correlationId = "corr-deliver-1")
        => new(proofOfDeliveryCaptured, _deliveredAt, correlationId);

    public CancelOrderRequirement Cancellation(
        string reason = "Customer changed their mind.",
        string correlationId = "corr-cancel-1")
        => new(reason, correlationId);

    private TrustableCode.SDK.TrustableModeling.Admission.AdmissionResult<Order> CreatedOrderResult()
        => OrderFactory.Create(CreateOrderRequest());
}
