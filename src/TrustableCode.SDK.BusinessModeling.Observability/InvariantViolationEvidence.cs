namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Observable evidence that a protected business invariant was violated or threatened.
/// </summary>
public sealed record InvariantViolationEvidence(
    string ModelName,
    string InvariantName,
    string Message,
    string? CorrelationId,
    DateTimeOffset ObservedAt) : IBusinessEvidence
{
    /// <summary>
    /// Stable classifier for invariant violation evidence.
    /// </summary>
    public string EvidenceType => "invariant-violation";
}
