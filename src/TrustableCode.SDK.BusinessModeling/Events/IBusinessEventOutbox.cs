namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Persists business events durably inside the same transaction as the model change so they can be published after commit.
/// </summary>
public interface IBusinessEventOutbox
{
    /// <summary>
    /// Stores outbox messages durably.
    /// </summary>
    Task EnqueueAsync(
        IReadOnlyList<BusinessEventOutboxMessage> messages,
        CancellationToken cancellationToken = default);
}
