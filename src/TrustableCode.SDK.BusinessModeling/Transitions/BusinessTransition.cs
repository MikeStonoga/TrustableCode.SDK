namespace TrustableCode.SDK.BusinessModeling.Transitions;

/// <summary>
/// Represents an explicit business transition between two meaningful states.
/// </summary>
public readonly record struct BusinessTransition<TState>(
    string Name,
    TState From,
    TState To);
