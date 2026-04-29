using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class OrderingQueryServiceTests
{
    [Fact]
    public void Find_order_should_return_persisted_snapshot()
    {
        var orders = new InMemoryOrderSnapshotStore();
        orders.Save(new OrderPersistenceSnapshot(
            OrderId: "order-1",
            CustomerId: "customer-1",
            Lines: [new OrderLine("sku-1", 1)],
            Status: OrderStatus.PaidAwaitingFulfillment));
        var queries = new OrderingQueryService(orders);

        var snapshot = queries.FindOrder("order-1");

        Assert.NotNull(snapshot);
        Assert.Equal(OrderStatus.PaidAwaitingFulfillment, snapshot.Status);
    }
}
