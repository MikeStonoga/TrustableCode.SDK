namespace TrustableCode.SDK.Samples.Ordering;

public sealed class OrderingQueryService(IOrderSnapshotStore orders)
{
    public OrderPersistenceSnapshot? FindOrder(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        return orders.Find(orderId);
    }
}
