namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Captures business evidence in memory for tests, samples, and local diagnostics.
/// </summary>
public sealed class InMemoryBusinessEvidenceSink : IBusinessEvidenceSink
{
    private readonly List<BusinessEvidence> _evidence = [];

    public IReadOnlyList<BusinessEvidence> Evidence => _evidence;

    public void Record(BusinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        _evidence.Add(evidence);
    }
}

