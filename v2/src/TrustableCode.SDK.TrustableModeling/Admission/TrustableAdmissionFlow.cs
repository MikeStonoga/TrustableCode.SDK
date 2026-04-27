using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Admission;

/// <summary>
/// Small application-layer helper for the common admission-then-transition flow.
/// </summary>
public static class TrustableAdmissionFlow
{
    public static AdmittedTransitionResult<TAccepted, TState> ExecuteTransition<TInput, TAccepted, TState>(
        BusinessAdmission<TInput, TAccepted> admission,
        TInput input,
        Func<TAccepted, TransitionExecutionResult<TState>> executeTransition)
        where TInput : notnull
        where TAccepted : notnull
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(executeTransition);

        var admitted = admission.Admit(input);
        if (!admitted.WasAccepted)
        {
            return AdmittedTransitionResult<TAccepted, TState>.Rejected(admitted);
        }

        var transition = executeTransition(admitted.Value!);
        return AdmittedTransitionResult<TAccepted, TState>.Executed(admitted, transition);
    }
}
