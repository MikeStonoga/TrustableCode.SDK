namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Observable evidence that a business model changed from one meaningful state to another.
/// </summary>
public sealed record BusinessTransitionEvidence<TState>(
    string ModelName,
    string TransitionName,
    TState PreviousState,
    TState CurrentState,
    string? CorrelationId,
    DateTimeOffset ObservedAt) : IBusinessEvidence
{
    /// <summary>
    /// Stable classifier for business transition evidence.
    /// </summary>
    public string EvidenceType => "business-transition";
}
