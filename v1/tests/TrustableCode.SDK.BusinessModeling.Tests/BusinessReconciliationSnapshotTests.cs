using TrustableCode.SDK.BusinessModeling.Reconciliation;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessReconciliationSnapshotTests
{
    [Fact]
    public void RepairRequired_should_mark_snapshot_as_repairable()
    {
        var snapshot = BusinessReconciliationSnapshot<string>.RepairRequired(
            "order-1",
            "Approved order is missing its approved projection.");

        Assert.True(snapshot.RequiresRepair);
        Assert.Equal("Approved order is missing its approved projection.", snapshot.RepairReason);
    }

    [Fact]
    public void Healthy_should_return_snapshot_without_repair()
    {
        var snapshot = BusinessReconciliationSnapshot<string>.Healthy("order-2");

        Assert.False(snapshot.RequiresRepair);
        Assert.Equal("No repair required.", snapshot.RepairReason);
    }
}
