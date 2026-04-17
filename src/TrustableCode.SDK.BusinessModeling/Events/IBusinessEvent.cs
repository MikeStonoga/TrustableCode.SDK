namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Represents a meaningful business occurrence emitted by the model after a legitimate state change.
/// </summary>
public interface IBusinessEvent
{
    /// <summary>
    /// Time when the business event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
