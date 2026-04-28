using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class OrderRehydrationTests
{
    [Fact]
    public void Rehydrate_should_restore_order_from_persistence_snapshot()
    {
        var snapshot = new OrderPersistenceSnapshot(
            OrderId: "order-42",
            CustomerId: "customer-42",
            Lines: [new OrderLine("sku-42", 3)],
            Status: OrderStatus.PaidAwaitingFulfillment);

        var order = Order.Rehydrate(snapshot);

        Assert.Equal("order-42", order.OrderId);
        Assert.Equal("customer-42", order.CustomerId);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, order.Status);
        Assert.Equal(new OrderLine("sku-42", 3), order.Lines.Single());
        Assert.Empty(order.Events);
        Assert.Empty(order.BusinessEvidence);
    }

    [Fact]
    public void Persistence_snapshot_should_capture_current_state_without_creation_evidence()
    {
        var order = Order.Rehydrate(OrderStatus.FulfilledReadyForShipping);

        var snapshot = OrderPersistenceSnapshot.From(order);

        Assert.Equal("order-1", snapshot.OrderId);
        Assert.Equal("customer-1", snapshot.CustomerId);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, snapshot.Status);
        Assert.Equal(order.Lines, snapshot.Lines);
    }
}
