using TrustableCode.SDK.BusinessModeling.Example.Ordering;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class OrderingReconciliationSampleTests
{
    [Fact]
    public void Reconciliation_snapshot_should_require_repair_when_projection_is_missing()
    {
        var order = Order.Create(new CreateOrderRequirement(OrderId.New(), OrderStatus.Paid));
        order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-recon-1"));

        var snapshot = OrderProjectionReconciliationSnapshot.Create(order, hasReadyForShippingProjection: false);

        Assert.True(snapshot.RequiresRepair);
        Assert.Equal("Order is ready for shipping but the ready-for-shipping projection is missing.", snapshot.RepairReason);
    }
}
