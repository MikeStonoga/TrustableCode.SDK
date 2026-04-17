using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Invariants;

/// <summary>
/// Collects multiple independent business rule violations before throwing.
/// </summary>
public sealed class BusinessNotification
{
    private readonly List<string> _violations = [];

    /// <summary>
    /// Gets the collected violation messages.
    /// </summary>
    public IReadOnlyList<string> Violations => _violations;

    /// <summary>
    /// Gets whether at least one business rule violation has been collected.
    /// </summary>
    public bool HasViolations => _violations.Count > 0;

    /// <summary>
    /// Executes a rule and collects its violation instead of failing immediately.
    /// </summary>
    public BusinessNotification Collect(IBusinessInvariantRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        try
        {
            rule.EnsureIsPreserved();
        }
        catch (BusinessRuleViolationException exception)
        {
            _violations.Add(exception.Message);
        }

        return this;
    }

    /// <summary>
    /// Lazily executes a rule only when the supplied condition is true.
    /// </summary>
    public BusinessNotification CollectIf(bool condition, Func<IBusinessInvariantRule> ruleFactory)
    {
        ArgumentNullException.ThrowIfNull(ruleFactory);

        if (!condition)
        {
            return this;
        }

        return Collect(ruleFactory.Invoke());
    }

    /// <summary>
    /// Throws when any collected violation exists.
    /// </summary>
    public void ThrowIfAny()
    {
        if (!HasViolations)
        {
            return;
        }

        throw new AggregatedBusinessRuleViolationException(_violations.AsReadOnly());
    }
}
