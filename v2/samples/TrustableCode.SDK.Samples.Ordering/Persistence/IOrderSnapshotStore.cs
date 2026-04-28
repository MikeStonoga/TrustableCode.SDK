namespace TrustableCode.SDK.Samples.Ordering;

public interface IOrderSnapshotStore
{
    OrderPersistenceSnapshot? Find(string orderId);

    void Save(OrderPersistenceSnapshot snapshot);
}
