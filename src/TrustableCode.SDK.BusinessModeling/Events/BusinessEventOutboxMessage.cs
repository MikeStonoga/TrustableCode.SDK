namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Durable outbox entry representing a business event that was persisted inside the same transaction as the model change.
/// </summary>
public sealed record BusinessEventOutboxMessage(
    Guid MessageId,
    string StreamName,
    string EventName,
    IBusinessEvent BusinessEvent,
    DateTimeOffset EnqueuedAt);
