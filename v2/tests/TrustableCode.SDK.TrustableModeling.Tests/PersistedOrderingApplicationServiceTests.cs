using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class PersistedOrderingApplicationServiceTests
{
    [Fact]
    public void Create_order_should_save_snapshot_and_enqueue_event()
    {
        var orders = new InMemoryOrderSnapshotStore();
        var outbox = new InMemoryOrderingOutbox();
        var evidence = new InMemoryBusinessEvidenceSink();
        var service = new PersistedOrderingApplicationService(
            orders,
            outbox,
            evidence,
            new InMemorySideEffectLifecycleStore());

        var result = service.CreateOrder(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: null,
            CorrelationId: "corr-persisted-create-1"));

        Assert.True(result.Succeeded);
        Assert.Equal(OrderStatus.PlacedAwaitingPayment, orders.Find("order-1")?.Status);
        Assert.Equal("OrderCreated", Assert.Single(outbox.Messages).EventName);
        Assert.Contains(evidence.Evidence, item => item.Name == "OrderCreatedEvidence");
    }

    [Fact]
    public void Create_order_should_record_rejection_without_snapshot_or_outbox()
    {
        var orders = new InMemoryOrderSnapshotStore();
        var outbox = new InMemoryOrderingOutbox();
        var evidence = new InMemoryBusinessEvidenceSink();
        var service = new PersistedOrderingApplicationService(
            orders,
            outbox,
            evidence,
            new InMemorySideEffectLifecycleStore());

        var result = service.CreateOrder(new ExternalCreateOrderRequest(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            RequestedStatus: "Delivered",
            CorrelationId: "corr-persisted-reject-create-1"));

        Assert.False(result.Succeeded);
        Assert.Null(orders.Find("order-1"));
        Assert.Empty(outbox.Messages);
        Assert.Contains(evidence.Evidence, item => item.Name == "OrderCreationRejectedEvidence");
    }

    [Fact]
    public void Prepare_for_shipping_should_load_snapshot_save_new_snapshot_and_enqueue_event()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var orders = new InMemoryOrderSnapshotStore();
        var outbox = new InMemoryOrderingOutbox();
        var evidence = new InMemoryBusinessEvidenceSink();
        orders.Save(scenario.PersistedOrder(OrderStatus.PaidAwaitingFulfillment));
        var service = new PersistedOrderingApplicationService(
            orders,
            outbox,
            evidence,
            new InMemorySideEffectLifecycleStore());

        var result = service.PrepareForShipping(
            "order-1",
            new ExternalPrepareOrderForShippingRequest(
                PaymentCaptured: true,
                StockReserved: true,
                RequestedStatus: "",
                CorrelationId: "corr-persisted-prepare-1"));

        Assert.True(result.Succeeded);
        Assert.Equal(TransitionExecutionStatus.Applied, result.TransitionStatus);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, orders.Find("order-1")?.Status);
        var message = Assert.Single(outbox.Messages);
        Assert.Equal("OrderPreparedForShipping", message.EventName);
        Assert.Equal("Order-order-1", message.StreamName);
        Assert.Contains(evidence.Evidence, item => item.Name == "NotifyFulfillmentPublishedEvidence");
    }

    [Fact]
    public void Ship_should_not_save_snapshot_or_enqueue_event_when_transition_rejects()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var orders = new InMemoryOrderSnapshotStore();
        var outbox = new InMemoryOrderingOutbox();
        orders.Save(scenario.PersistedOrder(OrderStatus.PaidAwaitingFulfillment));
        var service = new PersistedOrderingApplicationService(
            orders,
            outbox,
            new InMemoryBusinessEvidenceSink(),
            new InMemorySideEffectLifecycleStore());

        var result = service.Ship(
            "order-1",
            new ExternalShipOrderRequest(
                Carrier: "DHL",
                TrackingCode: "track-1",
                RequestedStatus: null,
                CorrelationId: "corr-persisted-ship-1"));

        Assert.False(result.Succeeded);
        Assert.Equal(TransitionExecutionStatus.Rejected, result.TransitionStatus);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, orders.Find("order-1")?.Status);
        Assert.Empty(outbox.Messages);
    }
}
