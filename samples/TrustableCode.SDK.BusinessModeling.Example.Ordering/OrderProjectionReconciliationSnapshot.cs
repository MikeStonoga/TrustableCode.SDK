using TrustableCode.SDK.BusinessModeling.Reconciliation;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed class OrderProjectionReconciliationSnapshot
{
    private OrderProjectionReconciliationSnapshot(BusinessReconciliationSnapshot<string> snapshot)
    {
        Snapshot = snapshot;
    }

    public BusinessReconciliationSnapshot<string> Snapshot { get; }

    public bool RequiresRepair => Snapshot.RequiresRepair;

    public string RepairReason => Snapshot.RepairReason;

    public static OrderProjectionReconciliationSnapshot Create(Order order, bool hasReadyForShippingProjection)
    {
        var isProjectionMissing = order.IsReadyForShipping && !hasReadyForShippingProjection;

        if (isProjectionMissing)
        {
            return new OrderProjectionReconciliationSnapshot(
                BusinessReconciliationSnapshot<string>.RepairRequired(
                    order.Id.ToString(),
                    "Order is ready for shipping but the ready-for-shipping projection is missing."));
        }

        return new OrderProjectionReconciliationSnapshot(
            BusinessReconciliationSnapshot<string>.Healthy(order.Id.ToString()));
    }
}
