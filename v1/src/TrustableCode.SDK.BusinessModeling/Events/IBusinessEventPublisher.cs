namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Publishes previously persisted outbox messages after the transaction that created them has committed successfully.
/// </summary>
public interface IBusinessEventPublisher
{
    /// <summary>
    /// Publishes the provided outbox messages.
    /// </summary>
    Task PublishAsync(
        IReadOnlyList<BusinessEventOutboxMessage> messages,
        CancellationToken cancellationToken = default);
}
