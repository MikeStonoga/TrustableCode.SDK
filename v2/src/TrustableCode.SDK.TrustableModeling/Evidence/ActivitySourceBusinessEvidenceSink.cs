using System.Diagnostics;

namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Emits business evidence through <see cref="ActivitySource"/> for tracing pipelines.
/// </summary>
public sealed class ActivitySourceBusinessEvidenceSink : IBusinessEvidenceSink
{
    private readonly ActivitySource _activitySource;

    public ActivitySourceBusinessEvidenceSink(ActivitySource activitySource)
    {
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
    }

    public void Record(BusinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        using var activity = _activitySource.StartActivity($"business.evidence.{evidence.Kind}", ActivityKind.Internal);
        if (activity is null)
        {
            return;
        }

        activity.SetTag(BusinessEvidenceFields.Name, evidence.Name);
        activity.SetTag(BusinessEvidenceFields.Kind, evidence.Kind.ToString());
        activity.SetTag(BusinessEvidenceFields.Message, evidence.Message);
        activity.SetTag(BusinessEvidenceFields.CorrelationId, evidence.CorrelationId);
        activity.SetTag(BusinessEvidenceFields.ObservedAt, evidence.ObservedAt.ToString("O"));

        foreach (var (key, value) in evidence.Metadata)
        {
            activity.SetTag($"{BusinessEvidenceFields.MetadataPrefix}{key}", value);
        }
    }
}
