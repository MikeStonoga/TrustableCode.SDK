namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// A named rule that must pass before a transition may change state.
/// </summary>
public sealed class TransitionPrecondition<TState, TContext>
    where TState : notnull
    where TContext : notnull
{
    public TransitionPrecondition(
        string code,
        string description,
        Func<TState, TContext, bool> isSatisfied,
        string rejectionReason)
    {
        Code = Require.Text(code, nameof(code));
        Description = Require.Text(description, nameof(description));
        IsSatisfied = isSatisfied ?? throw new ArgumentNullException(nameof(isSatisfied));
        RejectionReason = Require.Text(rejectionReason, nameof(rejectionReason));
    }

    public string Code { get; }

    public string Description { get; }

    public Func<TState, TContext, bool> IsSatisfied { get; }

    public string RejectionReason { get; }
}

