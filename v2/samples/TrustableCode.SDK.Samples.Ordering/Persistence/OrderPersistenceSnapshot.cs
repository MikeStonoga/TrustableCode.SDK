namespace TrustableCode.SDK.Samples.Ordering;

public sealed record OrderPersistenceSnapshot(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<OrderLine> Lines,
    OrderStatus Status)
{
    public static OrderPersistenceSnapshot From(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderPersistenceSnapshot(
            order.OrderId,
            order.CustomerId,
            order.Lines,
            order.Status);
    }
}
