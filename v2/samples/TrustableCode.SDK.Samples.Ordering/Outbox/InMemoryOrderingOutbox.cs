namespace TrustableCode.SDK.Samples.Ordering;

public sealed class InMemoryOrderingOutbox : IOrderingOutbox
{
    private readonly List<OrderingOutboxMessage> _messages = [];

    public IReadOnlyList<OrderingOutboxMessage> Messages => _messages;

    public void Enqueue(OrderingOutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Add(message);
    }
}
