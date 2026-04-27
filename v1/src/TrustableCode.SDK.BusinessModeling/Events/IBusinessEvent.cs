namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Represents a meaningful business occurrence emitted by the model after a legitimate state change.
/// </summary>
public interface IBusinessEvent
{
    /// <summary>
    /// Stable identifier for the business fact itself, allowing retries to recognize the same event across deliveries.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Time when the business event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
