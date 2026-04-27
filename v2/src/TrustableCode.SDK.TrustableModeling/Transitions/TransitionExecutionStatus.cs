namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Describes the outcome of executing a governed transition.
/// </summary>
public enum TransitionExecutionStatus
{
    Applied = 0,
    AlreadyApplied = 1,
    Rejected = 2
}

