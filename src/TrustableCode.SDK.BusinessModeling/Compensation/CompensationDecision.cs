using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Compensation;

/// <summary>
/// Represents an explicit decision that a compensating business action is now required.
/// </summary>
public sealed record CompensationDecision(
    Guid CompensationId,
    string Reason,
    DateTimeOffset DecidedAt)
{
    /// <summary>
    /// Creates a compensation decision with a durable identifier and a non-empty reason.
    /// </summary>
    public static CompensationDecision Create(string reason, DateTimeOffset decidedAt, Guid? compensationId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleViolationException("A compensation decision requires a reason.");
        }

        return new CompensationDecision(compensationId ?? Guid.NewGuid(), reason.Trim(), decidedAt);
    }
}
