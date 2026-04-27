namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Outcome of attempting to execute a governed side effect.
/// </summary>
public enum SideEffectExecutionStatus
{
    Executed = 0,
    AlreadyApplied = 1,
    Rejected = 2
}

