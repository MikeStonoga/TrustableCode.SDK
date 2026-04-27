namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Observable evidence that an external side effect was intended, attempted, completed, skipped, or failed.
/// </summary>
public sealed record SideEffectEvidence(
    string SideEffectName,
    string Outcome,
    string? BusinessOperation,
    string? CorrelationId,
    DateTimeOffset ObservedAt) : IBusinessEvidence
{
    /// <summary>
    /// Stable classifier for side-effect evidence.
    /// </summary>
    public string EvidenceType => "side-effect";
}
