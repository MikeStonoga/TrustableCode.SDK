namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Observable evidence that a durable compensating action was scheduled for later execution.
/// </summary>
public sealed record CompensationScheduledEvidence(
    string ModelName,
    string CompensationName,
    string CompensationId,
    string Reason,
    string? CorrelationId,
    DateTimeOffset ObservedAt) : IBusinessEvidence
{
    /// <summary>
    /// Stable classifier for compensation scheduling evidence.
    /// </summary>
    public string EvidenceType => "compensation-scheduled";
}
