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

        activity.SetTag("business.evidence.name", evidence.Name);
        activity.SetTag("business.evidence.kind", evidence.Kind.ToString());
        activity.SetTag("business.evidence.message", evidence.Message);
        activity.SetTag("business.evidence.correlation_id", evidence.CorrelationId);
        activity.SetTag("business.evidence.observed_at", evidence.ObservedAt.ToString("O"));

        foreach (var (key, value) in evidence.Metadata)
        {
            activity.SetTag($"business.metadata.{key}", value);
        }
    }
}

