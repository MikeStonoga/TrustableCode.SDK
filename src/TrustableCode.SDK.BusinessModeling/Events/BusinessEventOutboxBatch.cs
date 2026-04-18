namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Creates durable outbox messages from business event sources so state changes and event persistence can be committed together.
/// </summary>
public static class BusinessEventOutboxBatch
{
    /// <summary>
    /// Dequeues business events from the provided source and wraps them as outbox messages for durable persistence.
    /// </summary>
    public static IReadOnlyList<BusinessEventOutboxMessage> CreateFrom(
        string streamName,
        IBusinessEventSource source,
        DateTimeOffset? enqueuedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName);
        ArgumentNullException.ThrowIfNull(source);

        var timestamp = enqueuedAt ?? DateTimeOffset.UtcNow;
        var businessEvents = source.DequeueBusinessEvents();

        return businessEvents
            .Select(businessEvent => new BusinessEventOutboxMessage(
                MessageId: Guid.NewGuid(),
                StreamName: streamName,
                EventName: businessEvent.GetType().Name,
                BusinessEvent: businessEvent,
                EnqueuedAt: timestamp))
            .ToArray();
    }
}
