using TrustableCode.SDK.TrustableModeling.Evidence;

namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Helpers for common side-effect lifecycle progressions used by application services.
/// </summary>
public static class SideEffectLifecycleFlow
{
    public static SideEffectLifecycleRecord PlanPersistAndPublish<TContext>(
        this GovernedSideEffectLifecycle<TContext> lifecycle,
        TContext context,
        BusinessEvidenceRecorder recorder,
        string? correlationId = null)
        where TContext : notnull
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(recorder);

        var planned = lifecycle.Plan(context, correlationId);
        recorder.Record(planned.Evidence);

        var persisted = lifecycle.Persist(planned.IdempotencyKey, correlationId);
        recorder.Record(persisted.Evidence);

        var published = lifecycle.Publish(planned.IdempotencyKey, correlationId);
        recorder.Record(published.Evidence);

        return published;
    }
}
