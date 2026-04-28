using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class OrderSnapshotEntity
{
    public string OrderId { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public List<OrderLineEntity> Lines { get; set; } = [];
}
