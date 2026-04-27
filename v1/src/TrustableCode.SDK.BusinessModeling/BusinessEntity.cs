using TrustableCode.SDK.BusinessModeling.Events;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling;

/// <summary>
/// Base type for entities that need explicit invariant protection and business event collection.
/// </summary>
public abstract class BusinessEntity : IBusinessEventSource
{
    private readonly List<IBusinessEvent> _businessEvents = [];

    /// <summary>
    /// Gets the business events recorded by the entity since the last dequeue.
    /// </summary>
    public IReadOnlyList<IBusinessEvent> BusinessEvents => _businessEvents;

    /// <summary>
    /// Records a meaningful business event after a legitimate state change.
    /// </summary>
    protected void RecordBusinessEvent(IBusinessEvent businessEvent)
    {
        ArgumentNullException.ThrowIfNull(businessEvent);
        _businessEvents.Add(businessEvent);
    }

    /// <summary>
    /// Enforces a single invariant rule immediately.
    /// </summary>
    protected static void Ensure(IBusinessInvariantRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        rule.EnsureIsPreserved();
    }

    /// <summary>
    /// Enforces multiple independent invariant rules and fails with the aggregated violations.
    /// </summary>
    protected static void EnsureAll(Action<BusinessNotification> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var notification = new BusinessNotification();
        configure(notification);
        notification.ThrowIfAny();
    }

    /// <summary>
    /// Returns the currently recorded business events and clears the internal queue.
    /// </summary>
    public IReadOnlyList<IBusinessEvent> DequeueBusinessEvents()
    {
        var pendingEvents = _businessEvents.ToArray();
        _businessEvents.Clear();
        return pendingEvents;
    }
}
