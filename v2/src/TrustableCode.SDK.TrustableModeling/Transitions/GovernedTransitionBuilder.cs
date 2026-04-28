using TrustableCode.SDK.TrustableModeling.Invariants;

namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Fluent builder for declaring governed transitions without repeating constructor plumbing.
/// </summary>
public sealed class GovernedTransitionBuilder<TState, TContext>
    where TState : notnull
    where TContext : notnull
{
    private readonly string _name;
    private readonly List<TransitionPrecondition<TState, TContext>> _preconditions = [];
    private readonly List<IBusinessInvariant<TransitionContext<TState, TContext>>> _invariants = [];
    private readonly List<string> _producedEvents = [];
    private readonly List<string> _producedEvidence = [];
    private TState? _from;
    private TState? _to;
    private Func<TState>? _readState;
    private Action<TState>? _applyState;
    private TransitionRepetitionPolicy _repetitionPolicy = TransitionRepetitionPolicy.Reject;

    internal GovernedTransitionBuilder(string name)
    {
        _name = TrustableModeling.Require.Text(name, nameof(name));
    }

    public GovernedTransitionBuilder<TState, TContext> From(TState state)
    {
        _from = state;
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> To(TState state)
    {
        _to = state;
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> ReadState(Func<TState> readState)
    {
        _readState = readState ?? throw new ArgumentNullException(nameof(readState));
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> ApplyState(Action<TState> applyState)
    {
        _applyState = applyState ?? throw new ArgumentNullException(nameof(applyState));
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> State(GovernedState<TState> state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return ReadState(state.Read)
            .ApplyState(state.ApplyApproved);
    }

    public GovernedTransitionBuilder<TState, TContext> Require(TransitionPrecondition<TState, TContext> precondition)
    {
        ArgumentNullException.ThrowIfNull(precondition);
        _preconditions.Add(precondition);
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> Preserve(
        IBusinessInvariant<TransitionContext<TState, TContext>> invariant)
    {
        ArgumentNullException.ThrowIfNull(invariant);
        _invariants.Add(invariant);
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> Preserve(
        IEnumerable<IBusinessInvariant<TransitionContext<TState, TContext>>> invariants)
    {
        ArgumentNullException.ThrowIfNull(invariants);

        foreach (var invariant in invariants)
        {
            Preserve(invariant);
        }

        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> ProducesEvent(string eventName)
    {
        _producedEvents.Add(TrustableModeling.Require.Text(eventName, nameof(eventName)));
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> ProducesEvidence(string evidenceName)
    {
        _producedEvidence.Add(TrustableModeling.Require.Text(evidenceName, nameof(evidenceName)));
        return this;
    }

    public GovernedTransitionBuilder<TState, TContext> TreatRepetitionAsAlreadyApplied()
    {
        _repetitionPolicy = TransitionRepetitionPolicy.TreatAsAlreadyApplied;
        return this;
    }

    public GovernedTransition<TState, TContext> Build()
    {
        if (_from is null)
        {
            throw new InvalidOperationException($"Transition '{_name}' must declare a source state.");
        }

        if (_to is null)
        {
            throw new InvalidOperationException($"Transition '{_name}' must declare a target state.");
        }

        if (_readState is null)
        {
            throw new InvalidOperationException($"Transition '{_name}' must declare how to read current state.");
        }

        if (_applyState is null)
        {
            throw new InvalidOperationException($"Transition '{_name}' must declare how to apply approved state.");
        }

        return new GovernedTransition<TState, TContext>(
            name: _name,
            from: _from,
            to: _to,
            currentState: _readState,
            applyState: _applyState,
            preconditions: _preconditions,
            invariants: _invariants,
            producedEvents: _producedEvents,
            producedEvidence: _producedEvidence,
            repetitionPolicy: _repetitionPolicy);
    }
}
