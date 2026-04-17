namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Base record for business events captured by entities and aggregates.
/// </summary>
public abstract record BusinessEvent(DateTimeOffset OccurredAt) : IBusinessEvent;
