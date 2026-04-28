using Microsoft.EntityFrameworkCore;
using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class EfOrderSnapshotStore(OrderingDbContext db) : IOrderSnapshotStore
{
    public OrderPersistenceSnapshot? Find(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        var entity = db.Orders
            .Include(order => order.Lines)
            .SingleOrDefault(order => order.OrderId == orderId);

        return entity is null
            ? null
            : ToSnapshot(entity);
    }

    public void Save(OrderPersistenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var entity = db.Orders
            .Include(order => order.Lines)
            .SingleOrDefault(order => order.OrderId == snapshot.OrderId);

        if (entity is null)
        {
            db.Orders.Add(ToEntity(snapshot));
        }
        else
        {
            entity.CustomerId = snapshot.CustomerId;
            entity.Status = snapshot.Status;
            db.OrderLines.RemoveRange(entity.Lines);
            entity.Lines = snapshot.Lines
                .Select(line => new OrderLineEntity
                {
                    OrderId = snapshot.OrderId,
                    Sku = line.Sku,
                    Quantity = line.Quantity
                })
                .ToList();
        }

    }

    private static OrderPersistenceSnapshot ToSnapshot(OrderSnapshotEntity entity)
        => new(
            entity.OrderId,
            entity.CustomerId,
            entity.Lines
                .OrderBy(line => line.Id)
                .Select(line => new OrderLine(line.Sku, line.Quantity))
                .ToArray(),
            entity.Status);

    private static OrderSnapshotEntity ToEntity(OrderPersistenceSnapshot snapshot)
        => new()
        {
            OrderId = snapshot.OrderId,
            CustomerId = snapshot.CustomerId,
            Status = snapshot.Status,
            Lines = snapshot.Lines
                .Select(line => new OrderLineEntity
                {
                    OrderId = snapshot.OrderId,
                    Sku = line.Sku,
                    Quantity = line.Quantity
                })
                .ToList()
        };
}
