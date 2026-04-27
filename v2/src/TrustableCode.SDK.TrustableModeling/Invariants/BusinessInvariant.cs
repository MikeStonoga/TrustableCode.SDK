using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Invariants;

/// <summary>
/// Default executable invariant backed by a stable descriptor and a predicate.
/// </summary>
public sealed class BusinessInvariant<TContext> : BusinessInvariantRule<TContext>
    where TContext : notnull
{
    public BusinessInvariant(
        InvariantDescriptor descriptor,
        Func<TContext, bool> isPreserved,
        string violationMessage)
        : base(descriptor, isPreserved, violationMessage)
    {
    }
}
