namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// Context made available to invariants while a transition is being evaluated.
/// </summary>
public sealed record TransitionContext<TState, TInput>(
    string TransitionName,
    TState CurrentState,
    TState TargetState,
    TInput Input)
    where TState : notnull
    where TInput : notnull;

