using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Invariants;

/// <summary>
/// Base executable rule for business invariants and transition preconditions.
/// </summary>
public class BusinessInvariantRule<TContext> : IBusinessInvariant<TContext>
    where TContext : notnull
{
    private readonly Func<TContext, bool> _isPreserved;
    private readonly string _violationMessage;

    protected BusinessInvariantRule(
        InvariantDescriptor descriptor,
        Func<TContext, bool> isPreserved,
        string violationMessage)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _isPreserved = isPreserved ?? throw new ArgumentNullException(nameof(isPreserved));
        _violationMessage = Require.Text(violationMessage, nameof(violationMessage));
    }

    public InvariantDescriptor Descriptor { get; }

    public InvariantEvaluation Evaluate(TContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var isPreserved = _isPreserved(context);
        return new InvariantEvaluation(
            Descriptor.Code,
            Descriptor.Name,
            Descriptor.Severity,
            isPreserved,
            isPreserved ? Descriptor.Description : _violationMessage);
    }
}
