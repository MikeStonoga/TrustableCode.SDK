using TrustableCode.SDK.TrustableModeling.Evidence;

namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Structured result of a governed side effect attempt.
/// </summary>
public sealed record SideEffectExecutionResult
{
    public SideEffectExecutionResult(
        string sideEffectName,
        SideEffectExecutionStatus status,
        string? idempotencyKey,
        BusinessEvidence evidence,
        string? rejectionReason = null)
    {
        SideEffectName = Require.Text(sideEffectName, nameof(sideEffectName));
        Status = status;
        IdempotencyKey = idempotencyKey;
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        RejectionReason = rejectionReason;
    }

    public string SideEffectName { get; }

    public SideEffectExecutionStatus Status { get; }

    public string? IdempotencyKey { get; }

    public BusinessEvidence Evidence { get; }

    public string? RejectionReason { get; }
}

