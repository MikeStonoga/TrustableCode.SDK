namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Defines how a governed transition behaves when the model is already in the target state.
/// </summary>
public enum TransitionRepetitionPolicy
{
    Reject = 0,
    TreatAsAlreadyApplied = 1
}

