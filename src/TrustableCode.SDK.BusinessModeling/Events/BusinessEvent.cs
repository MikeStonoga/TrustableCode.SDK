namespace TrustableCode.SDK.BusinessModeling.Events;

/// <summary>
/// Base record for business events captured by entities and aggregates.
/// A business event communicates that a meaningful business fact happened and may matter to
/// downstream workflows, integrations, or other models. Unlike business evidence, which serves
/// observability and auditability, a business event represents business meaning that others may react to.
/// Meaningful transitions should prefer to emit one, then persist it through an outbox in the same transaction as the state change.
/// </summary>
public abstract record BusinessEvent(DateTimeOffset OccurredAt) : IBusinessEvent;
