using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Transitions;

/// <summary>
/// A named rule that must pass before a transition may change state.
/// </summary>
public abstract class TransitionPrecondition<TState, TContext> : BusinessInvariantRule<TransitionContext<TState, TContext>>
    where TState : notnull
    where TContext : notnull
{
    private readonly Func<TState, TContext, bool> _isSatisfied;

    protected TransitionPrecondition(
        string code,
        string description,
        Func<TState, TContext, bool> isSatisfied,
        string rejectionReason)
        : base(
            new InvariantDescriptor(
                code: code,
                name: code,
                description: description,
                severity: InvariantSeverity.Critical),
            context => isSatisfied(context.CurrentState, context.Input),
            rejectionReason)
    {
        Code = Descriptor.Code;
        Description = Descriptor.Description;
        _isSatisfied = isSatisfied ?? throw new ArgumentNullException(nameof(isSatisfied));
        RejectionReason = Require.Text(rejectionReason, nameof(rejectionReason));
    }

    public string Code { get; }

    public string Description { get; }

    public string RejectionReason { get; }

    public bool IsSatisfied(TState state, TContext context)
        => _isSatisfied(state, context);
}
