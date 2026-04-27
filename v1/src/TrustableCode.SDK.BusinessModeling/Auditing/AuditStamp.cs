namespace TrustableCode.SDK.BusinessModeling.Auditing;

/// <summary>
/// Immutable audit evidence describing who performed a business action and when it happened.
/// </summary>
public sealed class AuditStamp : ValueObject
{
    private AuditStamp(string actor, DateTimeOffset occurredAt, AuditAction action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);

        Actor = actor;
        OccurredAt = occurredAt;
        Action = action;
    }

    /// <summary>
    /// Gets the actor responsible for the business action.
    /// </summary>
    public string Actor { get; }

    /// <summary>
    /// Gets when the business action occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the semantic classification of the audited action.
    /// </summary>
    public AuditAction Action { get; }

    /// <summary>
    /// Creates an audit stamp for entity creation.
    /// </summary>
    public static AuditStamp CreateForCreation(string actor, DateTimeOffset occurredAt)
        => new(actor, occurredAt, AuditAction.Creation);

    /// <summary>
    /// Creates an audit stamp for entity modification.
    /// </summary>
    public static AuditStamp CreateForModification(string actor, DateTimeOffset occurredAt)
        => new(actor, occurredAt, AuditAction.Modification);

    /// <summary>
    /// Creates an audit stamp for entity deletion.
    /// </summary>
    public static AuditStamp CreateForDeletion(string actor, DateTimeOffset occurredAt)
        => new(actor, occurredAt, AuditAction.Deletion);

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Actor;
        yield return OccurredAt;
        yield return Action;
    }
}
