using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Transitions;

/// <summary>
/// Encapsulates an explicitly named transition that both guards and applies a state change.
/// The transition models the business move itself. If that move is significant to others, emit a business event.
/// If that move must be auditable or diagnosable, emit business evidence as a separate concern.
/// In event-first flows, the transition changes state, the model records the business event, and the application layer persists that event through an outbox transaction.
/// </summary>
public abstract class NamedBusinessTransition<TState>
    where TState : notnull
{
    private readonly Func<TState> _currentStateAccessor;
    private readonly Action<TState> _apply;

    /// <summary>
    /// Creates a named transition that validates and applies a state change atomically.
    /// </summary>
    protected NamedBusinessTransition(
        string name,
        TState to,
        Func<TState> currentStateAccessor,
        Action<TState> apply)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(currentStateAccessor);
        ArgumentNullException.ThrowIfNull(apply);

        Name = name;
        To = to;
        _currentStateAccessor = currentStateAccessor;
        _apply = apply;
    }

    /// <summary>
    /// Gets the business meaning of this transition.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the target state for this transition.
    /// </summary>
    public TState To { get; }

    /// <summary>
    /// Determines whether the current state may move through this transition.
    /// </summary>
    protected abstract bool CanTransitionFrom(TState currentState);

    /// <summary>
    /// Creates the business exception that explains why the transition was rejected.
    /// </summary>
    protected abstract BusinessRuleViolationException CreateException(TState currentState);

    /// <summary>
    /// Applies the transition after validating the current state.
    /// </summary>
    public BusinessTransition<TState> Execute()
    {
        var from = _currentStateAccessor();

        if (!CanTransitionFrom(from))
        {
            throw CreateException(from);
        }

        _apply(To);
        return new BusinessTransition<TState>(Name, from, To);
    }
}
