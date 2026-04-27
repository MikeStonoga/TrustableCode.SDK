namespace TrustableCode.SDK.BusinessModeling.Compensation;

/// <summary>
/// Durable request that records which subject now requires a compensating business action.
/// </summary>
public sealed record CompensationRequest<TSubjectId>(
    Guid CompensationId,
    string CompensationName,
    TSubjectId SubjectId,
    string Reason,
    DateTimeOffset RequestedAt)
{
    /// <summary>
    /// Creates a durable compensation request from an explicit compensation decision.
    /// </summary>
    public static CompensationRequest<TSubjectId> CreateFor(
        TSubjectId subjectId,
        string compensationName,
        CompensationDecision decision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compensationName);
        ArgumentNullException.ThrowIfNull(subjectId);
        ArgumentNullException.ThrowIfNull(decision);

        return new CompensationRequest<TSubjectId>(
            decision.CompensationId,
            compensationName.Trim(),
            subjectId,
            decision.Reason,
            decision.DecidedAt);
    }
}
