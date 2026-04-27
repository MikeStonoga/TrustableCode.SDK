using TrustableCode.SDK.TrustableModeling.Evidence;

namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Durable business record for the lifecycle of a side effect.
/// </summary>
public sealed record SideEffectLifecycleRecord
{
    public SideEffectLifecycleRecord(
        string sideEffectName,
        string idempotencyKey,
        SideEffectLifecycleStatus status,
        BusinessEvidence evidence,
        IReadOnlyList<BusinessEvidence>? history = null)
    {
        SideEffectName = Require.Text(sideEffectName, nameof(sideEffectName));
        IdempotencyKey = Require.Text(idempotencyKey, nameof(idempotencyKey));
        Status = status;
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        History = history ?? new List<BusinessEvidence> { Evidence };
    }

    public string SideEffectName { get; }

    public string IdempotencyKey { get; }

    public SideEffectLifecycleStatus Status { get; }

    public BusinessEvidence Evidence { get; }

    public IReadOnlyList<BusinessEvidence> History { get; }

    public SideEffectLifecycleRecord AdvanceTo(SideEffectLifecycleStatus status, BusinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var history = new List<BusinessEvidence>(History) { evidence };
        return new SideEffectLifecycleRecord(SideEffectName, IdempotencyKey, status, evidence, history);
    }
}
