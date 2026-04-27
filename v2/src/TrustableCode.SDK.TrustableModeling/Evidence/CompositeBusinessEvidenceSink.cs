namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Dispatches business evidence to multiple sinks.
/// </summary>
public sealed class CompositeBusinessEvidenceSink : IBusinessEvidenceSink
{
    private readonly IReadOnlyList<IBusinessEvidenceSink> _sinks;

    public CompositeBusinessEvidenceSink(IEnumerable<IBusinessEvidenceSink> sinks)
    {
        _sinks = Require.NotEmpty(sinks, nameof(sinks));
    }

    public void Record(BusinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        foreach (var sink in _sinks)
        {
            sink.Record(evidence);
        }
    }
}

