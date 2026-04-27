using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class OrderingApplicationServiceTests
{
    [Fact]
    public void Create_order_should_admit_input_and_publish_creation_evidence()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var sink = new InMemoryBusinessEvidenceSink();
        var service = new OrderingApplicationService(sink, new InMemorySideEffectLifecycleStore());

        var result = service.CreateOrder(scenario.CreateOrderRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(OrderStatus.PlacedAwaitingPayment, result.CurrentStatus);
        Assert.NotNull(result.Order);
        Assert.Contains("OrderCreated", result.ProducedEvents);
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "OrderCreatedEvidence");
    }

    [Fact]
    public void Create_order_should_publish_admission_rejection_evidence()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var sink = new InMemoryBusinessEvidenceSink();
        var service = new OrderingApplicationService(sink, new InMemorySideEffectLifecycleStore());

        var result = service.CreateOrder(scenario.CreateOrderRequest(requestedStatus: "Delivered"));

        Assert.False(result.Succeeded);
        Assert.False(result.WasAccepted);
        Assert.Contains(
            "External callers may create an order, but may not submit an arbitrary initial order status.",
            result.RejectionReasons);
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "OrderCreationRejectedEvidence");
    }

    [Fact]
    public void Prepare_for_shipping_should_apply_transition_and_publish_fulfillment_lifecycle()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var sink = new InMemoryBusinessEvidenceSink();
        var lifecycleStore = new InMemorySideEffectLifecycleStore();
        var service = new OrderingApplicationService(sink, lifecycleStore);
        var order = scenario.RehydratedOrder(OrderStatus.PaidAwaitingFulfillment);

        var result = service.PrepareForShipping(order, new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "",
            CorrelationId: "corr-app-prepare-1"));

        Assert.True(result.Succeeded);
        Assert.Equal(TransitionExecutionStatus.Applied, result.TransitionStatus);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, result.CurrentStatus);
        Assert.Equal(SideEffectLifecycleStatus.Published, result.SideEffectLifecycle?.Status);
        Assert.Equal(SideEffectLifecycleStatus.Published, lifecycleStore.All().Single().Status);
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "NotifyFulfillmentPlannedEvidence");
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "NotifyFulfillmentPersistedEvidence");
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "NotifyFulfillmentPublishedEvidence");
    }

    [Fact]
    public void Cancel_should_publish_transition_rejection_evidence_when_order_is_shipped()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var sink = new InMemoryBusinessEvidenceSink();
        var service = new OrderingApplicationService(sink, new InMemorySideEffectLifecycleStore());
        var order = scenario.RehydratedOrder(OrderStatus.ShippedWaitingDelivery);

        var result = service.Cancel(order, new ExternalCancelOrderRequest(
            Reason: "Customer requested cancellation.",
            RequestedStatus: null,
            CorrelationId: "corr-app-cancel-1"));

        Assert.False(result.Succeeded);
        Assert.True(result.WasAccepted);
        Assert.Equal(TransitionExecutionStatus.Rejected, result.TransitionStatus);
        Assert.Equal(OrderStatus.ShippedWaitingDelivery, result.CurrentStatus);
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "ShippedOrdersCannotBeCancelledViolation");
    }
}
