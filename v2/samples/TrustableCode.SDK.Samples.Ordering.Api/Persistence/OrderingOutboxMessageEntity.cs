namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class OrderingOutboxMessageEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string StreamName { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public DateTimeOffset EnqueuedAt { get; set; } = DateTimeOffset.UtcNow;
}
