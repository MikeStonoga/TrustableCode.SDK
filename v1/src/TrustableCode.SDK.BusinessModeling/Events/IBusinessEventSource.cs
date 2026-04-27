namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Represents a source capable of yielding the business events it has accumulated.
/// </summary>
public interface IBusinessEventSource
{
    /// <summary>
    /// Returns the currently pending business events and clears the internal queue.
    /// </summary>
    IReadOnlyList<IBusinessEvent> DequeueBusinessEvents();
}
