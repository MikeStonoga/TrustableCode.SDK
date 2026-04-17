using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessInvariantManifestTests
{
    [Fact]
    public void Manifest_should_expose_invariant_descriptions_by_semantic_key()
    {
        var manifest = BusinessInvariantManifest<OrderInvariant>.Create(builder => builder
            .Add(OrderInvariant.PaymentMustBeCaptured, "Payment must be captured before shipping.")
            .Add(OrderInvariant.StockMustBeReserved, "Stock must be reserved before shipping."));

        Assert.Equal("Payment must be captured before shipping.", manifest[OrderInvariant.PaymentMustBeCaptured]);
        Assert.True(manifest.TryGetDescription(OrderInvariant.StockMustBeReserved, out var description));
        Assert.Equal("Stock must be reserved before shipping.", description);
    }

    private enum OrderInvariant
    {
        PaymentMustBeCaptured = 1,
        StockMustBeReserved = 2
    }
}
