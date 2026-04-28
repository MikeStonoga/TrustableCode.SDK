namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class OrderLineEntity
{
    public int Id { get; set; }

    public string OrderId { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
