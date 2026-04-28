using System.Collections.Concurrent;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed class InMemoryOrderSnapshotStore : IOrderSnapshotStore
{
    private readonly ConcurrentDictionary<string, OrderPersistenceSnapshot> _snapshots = new();

    public OrderPersistenceSnapshot? Find(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        return _snapshots.TryGetValue(orderId, out var snapshot)
            ? snapshot
            : null;
    }

    public void Save(OrderPersistenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _snapshots[snapshot.OrderId] = snapshot;
    }
}
