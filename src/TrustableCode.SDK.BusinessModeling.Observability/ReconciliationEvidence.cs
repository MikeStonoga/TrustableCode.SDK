namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Observable evidence that reconciliation detected healthy alignment or a repair-worthy divergence.
/// </summary>
public sealed record ReconciliationEvidence(
    string ModelName,
    string SubjectId,
    bool RequiresRepair,
    string RepairReason,
    string? CorrelationId,
    DateTimeOffset ObservedAt) : IBusinessEvidence
{
    /// <summary>
    /// Stable classifier for reconciliation evidence.
    /// </summary>
    public string EvidenceType => "reconciliation";
}
