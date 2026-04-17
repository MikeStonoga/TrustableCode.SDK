namespace TrustableCode.SDK.BusinessModeling.Exceptions;

/// <summary>
/// Base exception for violated business invariants and illegal business transitions.
/// </summary>
public class BusinessRuleViolationException : Exception
{
    /// <summary>
    /// Creates an exception for a violated business rule.
    /// </summary>
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates an exception for a violated business rule with an inner cause.
    /// </summary>
    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
