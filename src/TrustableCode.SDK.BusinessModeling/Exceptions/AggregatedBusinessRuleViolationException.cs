namespace TrustableCode.SDK.BusinessModeling.Exceptions;

/// <summary>
/// Represents a set of business rule violations collected together before failing.
/// </summary>
public sealed class AggregatedBusinessRuleViolationException : BusinessRuleViolationException
{
    /// <summary>
    /// Creates an exception containing multiple violated business rules.
    /// </summary>
    public AggregatedBusinessRuleViolationException(IReadOnlyList<string> violations)
        : base(violations.Count > 0
            ? violations[0]
            : "One or more business rules were violated.")
    {
        Violations = violations;
    }

    /// <summary>
    /// Gets the collected violation messages.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }
}
