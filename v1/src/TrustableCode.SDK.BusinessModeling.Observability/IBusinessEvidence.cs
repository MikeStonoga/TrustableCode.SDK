namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Evidence emitted to make meaningful business behavior observable and auditable.
/// </summary>
public interface IBusinessEvidence
{
    /// <summary>
    /// Stable classifier for the kind of business evidence.
    /// </summary>
    string EvidenceType { get; }

    /// <summary>
    /// Time when the evidence was observed or produced.
    /// </summary>
    DateTimeOffset ObservedAt { get; }
}
