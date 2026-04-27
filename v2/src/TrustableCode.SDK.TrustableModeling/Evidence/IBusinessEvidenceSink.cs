namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Receives structured business evidence produced by models, admissions, transitions, or side effects.
/// </summary>
public interface IBusinessEvidenceSink
{
    void Record(BusinessEvidence evidence);
}

