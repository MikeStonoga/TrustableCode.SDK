using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Modeling;

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
    private readonly IReadOnlyList<IBusinessInvariant<TransitionContext<TState, TContext>>> _invariants;
    private readonly IReadOnlyList<string> _producedEvents;
    private readonly IReadOnlyList<string> _producedEvidence;

    public GovernedTransition(
        string name,
        TState from,
        TState to,
        Func<TState> currentState,
        Action<TState> applyState,
        IEnumerable<TransitionPrecondition<TState, TContext>>? preconditions = null,
        IEnumerable<IBusinessInvariant<TransitionContext<TState, TContext>>>? invariants = null,
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
        _invariants = invariants?.ToArray() ?? [];
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
        var rejectionEvidence = new List<BusinessEvidence>();

        if (!EqualityComparer<TState>.Default.Equals(previousState, From))
        {
            var message = $"Transition '{Name}' expects state '{From}' but current state is '{previousState}'.";
            rejectionReasons.Add(message);
            rejectionEvidence.Add(new BusinessEvidence(
                name: $"{Name}Rejected",
                kind: EvidenceKind.InvariantViolation,
                message: message,
                metadata: new Dictionary<string, string>
                {
                    ["transition.name"] = Name,
                    ["transition.expected_state"] = From.ToString() ?? string.Empty,
                    ["transition.current_state"] = previousState.ToString() ?? string.Empty,
                    ["transition.target_state"] = To.ToString() ?? string.Empty
                }));
        }

        var transitionContext = new TransitionContext<TState, TContext>(Name, previousState, To, context);
        foreach (var precondition in _preconditions)
        {
            var evaluation = precondition.Evaluate(transitionContext);
            if (!evaluation.IsPreserved)
            {
                rejectionReasons.Add(evaluation.Message);
                rejectionEvidence.Add(evaluation.ToEvidence());
            }
        }

        foreach (var invariantViolation in new InvariantSet<TransitionContext<TState, TContext>>(_invariants)
            .FindViolations(transitionContext))
        {
            rejectionReasons.Add(invariantViolation.Message);
            rejectionEvidence.Add(invariantViolation.ToEvidence());
        }

        if (rejectionReasons.Count > 0)
        {
            return new TransitionExecutionResult<TState>(
                Name,
                previousState,
                previousState,
                TransitionExecutionStatus.Rejected,
                rejectionReasons,
                rejectionEvidence);
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
