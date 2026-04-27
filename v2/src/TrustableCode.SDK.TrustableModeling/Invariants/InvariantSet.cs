namespace TrustableCode.SDK.TrustableModeling.Invariants;

/// <summary>
/// Evaluates a group of invariants and exposes only the violations when needed.
/// </summary>
public sealed class InvariantSet<TContext>
    where TContext : notnull
{
    private readonly IReadOnlyList<IBusinessInvariant<TContext>> _invariants;

    public InvariantSet(IEnumerable<IBusinessInvariant<TContext>> invariants)
    {
        ArgumentNullException.ThrowIfNull(invariants);
        _invariants = invariants.ToArray();
    }

    public IReadOnlyList<InvariantEvaluation> Evaluate(TContext context)
        => _invariants.Select(invariant => invariant.Evaluate(context)).ToArray();

    public IReadOnlyList<InvariantEvaluation> FindViolations(TContext context)
        => Evaluate(context).Where(evaluation => !evaluation.IsPreserved).ToArray();
}

