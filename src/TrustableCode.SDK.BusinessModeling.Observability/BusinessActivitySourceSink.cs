using System.Diagnostics;

namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Optional adapter that emits business evidence through <see cref="ActivitySource"/> for OpenTelemetry-style pipelines.
/// </summary>
public sealed class BusinessActivitySourceSink : IBusinessEvidenceSink
{
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Creates an activity-backed sink for business evidence.
    /// </summary>
    public BusinessActivitySourceSink(ActivitySource activitySource)
    {
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
    }

    /// <inheritdoc />
    public Task WriteAsync(IBusinessEvidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        using var activity = _activitySource.StartActivity($"business.{evidence.EvidenceType}", ActivityKind.Internal);
        if (activity is null)
        {
            return Task.CompletedTask;
        }

        activity.SetTag("business.evidence.type", evidence.EvidenceType);
        activity.SetTag("business.evidence.observed_at", evidence.ObservedAt);

        switch (evidence)
        {
            case IBusinessTransitionEvidence transition:
                activity.SetTag("business.model.name", transition.ModelName);
                activity.SetTag("business.transition.name", transition.TransitionName);
                activity.SetTag("business.transition.from", transition.PreviousStateText);
                activity.SetTag("business.transition.to", transition.CurrentStateText);
                activity.SetTag("business.correlation_id", transition.CorrelationId);
                break;

            case InvariantViolationEvidence violation:
                activity.SetTag("business.model.name", violation.ModelName);
                activity.SetTag("business.invariant.name", violation.InvariantName);
                activity.SetTag("business.violation.message", violation.Message);
                activity.SetTag("business.correlation_id", violation.CorrelationId);
                break;

            case SideEffectEvidence sideEffect:
                activity.SetTag("business.side_effect.name", sideEffect.SideEffectName);
                activity.SetTag("business.side_effect.outcome", sideEffect.Outcome);
                activity.SetTag("business.operation.name", sideEffect.BusinessOperation);
                activity.SetTag("business.correlation_id", sideEffect.CorrelationId);
                break;

            case ReconciliationEvidence reconciliation:
                activity.SetTag("business.model.name", reconciliation.ModelName);
                activity.SetTag("business.subject.id", reconciliation.SubjectId);
                activity.SetTag("business.reconciliation.requires_repair", reconciliation.RequiresRepair);
                activity.SetTag("business.reconciliation.reason", reconciliation.RepairReason);
                activity.SetTag("business.correlation_id", reconciliation.CorrelationId);
                break;

            case CompensationScheduledEvidence compensation:
                activity.SetTag("business.model.name", compensation.ModelName);
                activity.SetTag("business.compensation.name", compensation.CompensationName);
                activity.SetTag("business.compensation.id", compensation.CompensationId);
                activity.SetTag("business.compensation.reason", compensation.Reason);
                activity.SetTag("business.correlation_id", compensation.CorrelationId);
                break;
        }

        return Task.CompletedTask;
    }
}
