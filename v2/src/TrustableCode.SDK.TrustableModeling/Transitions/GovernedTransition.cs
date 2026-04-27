namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Executes one business transition through named preconditions, a state application step, and declared outputs.
/// </summary>
public sealed class GovernedTransition<TState, TContext>
    where TState : notnull
    where TContext : notnull
{
    private readonly Func<TState> _currentState;
    private readonly Action<TState> _applyState;
    private readonly IReadOnlyList<TransitionPrecondition<TState, TContext>> _preconditions;
    private readonly IReadOnlyList<string> _producedEvents;
    private readonly IReadOnlyList<string> _producedEvidence;

    public GovernedTransition(
        string name,
        TState from,
        TState to,
        Func<TState> currentState,
        Action<TState> applyState,
        IEnumerable<TransitionPrecondition<TState, TContext>>? preconditions = null,
        IEnumerable<string>? producedEvents = null,
        IEnumerable<string>? producedEvidence = null,
        TransitionRepetitionPolicy repetitionPolicy = TransitionRepetitionPolicy.Reject)
    {
        Name = Require.Text(name, nameof(name));
        From = from;
        To = to;
        _currentState = currentState ?? throw new ArgumentNullException(nameof(currentState));
        _applyState = applyState ?? throw new ArgumentNullException(nameof(applyState));
        _preconditions = preconditions?.ToArray() ?? [];
        _producedEvents = Require.TextList(producedEvents);
        _producedEvidence = Require.TextList(producedEvidence);
        RepetitionPolicy = repetitionPolicy;
    }

    public string Name { get; }

    public TState From { get; }

    public TState To { get; }

    public TransitionRepetitionPolicy RepetitionPolicy { get; }

    public TransitionExecutionResult<TState> Execute(TContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var previousState = _currentState();
        if (EqualityComparer<TState>.Default.Equals(previousState, To)
            && RepetitionPolicy == TransitionRepetitionPolicy.TreatAsAlreadyApplied)
        {
            return new TransitionExecutionResult<TState>(
                Name,
                previousState,
                previousState,
                TransitionExecutionStatus.AlreadyApplied,
                producedEvents: _producedEvents,
                producedEvidence: _producedEvidence);
        }

        var rejectionReasons = new List<string>();

        if (!EqualityComparer<TState>.Default.Equals(previousState, From))
        {
            rejectionReasons.Add($"Transition '{Name}' expects state '{From}' but current state is '{previousState}'.");
        }

        foreach (var precondition in _preconditions)
        {
            if (!precondition.IsSatisfied(previousState, context))
            {
                rejectionReasons.Add(precondition.RejectionReason);
            }
        }

        if (rejectionReasons.Count > 0)
        {
            return new TransitionExecutionResult<TState>(
                Name,
                previousState,
                previousState,
                TransitionExecutionStatus.Rejected,
                rejectionReasons);
        }

        _applyState(To);

        return new TransitionExecutionResult<TState>(
            Name,
            previousState,
            To,
            TransitionExecutionStatus.Applied,
            producedEvents: _producedEvents,
            producedEvidence: _producedEvidence);
    }
}

