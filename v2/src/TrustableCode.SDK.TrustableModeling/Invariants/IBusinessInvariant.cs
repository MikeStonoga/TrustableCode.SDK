using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Invariants;

/// <summary>
/// Executable business truth that must remain preserved for a given context.
/// </summary>
public interface IBusinessInvariant<in TContext>
    where TContext : notnull
{
    InvariantDescriptor Descriptor { get; }

    InvariantEvaluation Evaluate(TContext context);
}

