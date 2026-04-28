namespace TrustableCode.SDK.Samples.Ordering;

public interface IOrderingOutbox
{
    void Enqueue(OrderingOutboxMessage message);
}
