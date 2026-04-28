using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.Samples.Ordering.Api.Models;

public sealed record OrderResponse(
    string OrderId,
    string CustomerId,
    OrderStatus Status,
    IReadOnlyList<OrderLine> Lines)
{
    public static OrderResponse From(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderResponse(
            order.OrderId,
            order.CustomerId,
            order.Status,
            order.Lines);
    }

    public static OrderResponse From(OrderPersistenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new OrderResponse(
            snapshot.OrderId,
            snapshot.CustomerId,
            snapshot.Status,
            snapshot.Lines.ToArray());
    }
}
