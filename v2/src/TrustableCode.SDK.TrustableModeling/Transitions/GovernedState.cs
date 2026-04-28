namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Holds state that should be changed by governed transitions instead of direct assignment.
/// </summary>
public sealed class GovernedState<TState>
    where TState : notnull
{
    public GovernedState(TState initialState)
    {
        Current = initialState;
    }

    public TState Current { get; private set; }

    public static GovernedState<TState> Create(TState initialState)
        => new(initialState);

    public TState Read()
        => Current;

    public void ApplyApproved(TState state)
        => Current = state;
}
