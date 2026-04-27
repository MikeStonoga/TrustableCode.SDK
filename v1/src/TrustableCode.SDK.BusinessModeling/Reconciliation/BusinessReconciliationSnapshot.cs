namespace TrustableCode.SDK.BusinessModeling.Reconciliation;

/// <summary>
/// Snapshot used to compare current business reality against another representation and decide whether repair is required.
/// </summary>
public sealed class BusinessReconciliationSnapshot<TSubjectId> : ValueObject
    where TSubjectId : notnull
{
    private BusinessReconciliationSnapshot(TSubjectId subjectId, bool requiresRepair, string reason)
    {
        SubjectId = subjectId;
        RequiresRepair = requiresRepair;
        RepairReason = reason;
    }

    /// <summary>
    /// Gets the identifier of the reconciled subject.
    /// </summary>
    public TSubjectId SubjectId { get; }

    /// <summary>
    /// Gets whether the subject requires a repair action.
    /// </summary>
    public bool RequiresRepair { get; }

    /// <summary>
    /// Gets the reason for the repair decision.
    /// </summary>
    public string RepairReason { get; }

    /// <summary>
    /// Creates a healthy snapshot with no repair required.
    /// </summary>
    public static BusinessReconciliationSnapshot<TSubjectId> Healthy(TSubjectId subjectId)
        => new(subjectId, false, "No repair required.");

    /// <summary>
    /// Creates a snapshot that requires repair for the given reason.
    /// </summary>
    public static BusinessReconciliationSnapshot<TSubjectId> RepairRequired(TSubjectId subjectId, string repairReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repairReason);
        return new(subjectId, true, repairReason);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SubjectId;
        yield return RequiresRepair;
        yield return RepairReason;
    }
}
