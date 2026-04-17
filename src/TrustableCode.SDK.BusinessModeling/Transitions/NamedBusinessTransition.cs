using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Transitions;

/// <summary>
/// Encapsulates an explicitly named transition that both guards and applies a state change.
/// </summary>
public sealed class NamedBusinessTransition<TState>
    where TState : notnull
{
    private readonly Func<TState> _currentStateAccessor;
    private readonly Action<TState> _apply;
    private readonly Func<TState, bool> _canTransition;
    private readonly Func<TState, BusinessRuleViolationException> _exceptionFactory;

    /// <summary>
    /// Creates a named transition that validates and applies a state change atomically.
    /// </summary>
    public NamedBusinessTransition(
        string name,
        TState to,
        Func<TState> currentStateAccessor,
        Action<TState> apply,
        Func<TState, bool> canTransition,
        Func<TState, BusinessRuleViolationException> exceptionFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(currentStateAccessor);
        ArgumentNullException.ThrowIfNull(apply);
        ArgumentNullException.ThrowIfNull(canTransition);
        ArgumentNullException.ThrowIfNull(exceptionFactory);

        Name = name;
        To = to;
        _currentStateAccessor = currentStateAccessor;
        _apply = apply;
        _canTransition = canTransition;
        _exceptionFactory = exceptionFactory;
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
    /// Applies the transition after validating the current state.
    /// </summary>
    public BusinessTransition<TState> Execute()
    {
        var from = _currentStateAccessor();

        if (!_canTransition(from))
        {
            throw _exceptionFactory(from);
        }

        _apply(To);
        return new BusinessTransition<TState>(Name, from, To);
    }
}
