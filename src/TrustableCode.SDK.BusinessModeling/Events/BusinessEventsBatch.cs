namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Creates one batched collection from multiple business event sources.
/// </summary>
public static class BusinessEventsBatch
{
    /// <summary>
    /// Dequeues and combines business events from all provided sources.
    /// </summary>
    public static IReadOnlyList<IBusinessEvent> CreateFrom(params IBusinessEventSource[] sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var events = new List<IBusinessEvent>();

        foreach (var source in sources)
        {
            if (source is null)
            {
                continue;
            }

            events.AddRange(source.DequeueBusinessEvents());
        }

        return events.AsReadOnly();
    }
}
