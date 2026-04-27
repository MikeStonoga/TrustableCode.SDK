using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Admission;

/// <summary>
/// Result of admitting external input and, when accepted, executing the associated transition.
/// </summary>
public sealed record AdmittedTransitionResult<TAccepted, TState>
    where TAccepted : notnull
    where TState : notnull
{
    private AdmittedTransitionResult(
        AdmissionResult<TAccepted> admission,
        TransitionExecutionResult<TState>? transition)
    {
        Admission = admission ?? throw new ArgumentNullException(nameof(admission));
        Transition = transition;
    }

    public AdmissionResult<TAccepted> Admission { get; }

    public TransitionExecutionResult<TState>? Transition { get; }

    public bool WasAccepted => Admission.WasAccepted;

    public bool WasApplied => Transition?.WasApplied == true;

    public IReadOnlyList<string> RejectionReasons =>
        WasAccepted
            ? Transition?.RejectionReasons ?? []
            : Admission.RejectionReasons;

    public IReadOnlyList<BusinessEvidence> RejectionEvidence =>
        WasAccepted
            ? Transition?.RejectionEvidence ?? []
            : Admission.RejectionEvidence;

    public static AdmittedTransitionResult<TAccepted, TState> Rejected(AdmissionResult<TAccepted> admission)
        => new(admission, transition: null);

    public static AdmittedTransitionResult<TAccepted, TState> Executed(
        AdmissionResult<TAccepted> admission,
        TransitionExecutionResult<TState> transition)
        => new(admission, transition);
}
