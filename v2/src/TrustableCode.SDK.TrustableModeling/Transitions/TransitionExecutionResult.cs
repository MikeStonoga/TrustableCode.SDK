namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Captures the semantic outcome of a governed transition attempt.
/// </summary>
public sealed record TransitionExecutionResult<TState>
    where TState : notnull
{
    public TransitionExecutionResult(
        string transitionName,
        TState previousState,
        TState currentState,
        TransitionExecutionStatus status,
        IEnumerable<string>? rejectionReasons = null,
        IEnumerable<string>? producedEvents = null,
        IEnumerable<string>? producedEvidence = null)
    {
        TransitionName = Require.Text(transitionName, nameof(transitionName));
        PreviousState = previousState;
        CurrentState = currentState;
        Status = status;
        RejectionReasons = Require.TextList(rejectionReasons);
        ProducedEvents = Require.TextList(producedEvents);
        ProducedEvidence = Require.TextList(producedEvidence);
    }

    public string TransitionName { get; }

    public TState PreviousState { get; }

    public TState CurrentState { get; }

    public TransitionExecutionStatus Status { get; }

    public bool WasApplied => Status is TransitionExecutionStatus.Applied or TransitionExecutionStatus.AlreadyApplied;

    public IReadOnlyList<string> RejectionReasons { get; }

    public IReadOnlyList<string> ProducedEvents { get; }

    public IReadOnlyList<string> ProducedEvidence { get; }
}

