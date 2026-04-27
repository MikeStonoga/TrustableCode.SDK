namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Small helper for recording evidence collections without exposing sink details to domain code.
/// </summary>
public sealed class BusinessEvidenceRecorder
{
    private readonly IBusinessEvidenceSink _sink;

    public BusinessEvidenceRecorder(IBusinessEvidenceSink sink)
    {
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    }

    public void Record(BusinessEvidence evidence)
        => _sink.Record(evidence);

    public void RecordMany(IEnumerable<BusinessEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        foreach (var item in evidence)
        {
            Record(item);
        }
    }
}

