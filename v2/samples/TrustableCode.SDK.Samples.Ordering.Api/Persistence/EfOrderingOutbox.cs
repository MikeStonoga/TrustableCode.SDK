using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class EfOrderingOutbox(OrderingDbContext db) : IOrderingOutbox
{
    public void Enqueue(OrderingOutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        db.OutboxMessages.Add(new OrderingOutboxMessageEntity
        {
            StreamName = message.StreamName,
            EventName = message.EventName,
            OrderId = message.OrderId,
            CorrelationId = message.CorrelationId
        });
    }
}
