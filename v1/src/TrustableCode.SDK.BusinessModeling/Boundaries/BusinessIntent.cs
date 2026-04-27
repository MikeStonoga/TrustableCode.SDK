namespace TrustableCode.SDK.BusinessModeling.Boundaries;

/// <summary>
/// Represents an explicit business intention entering the model, carrying meaning rather than raw field mutation.
/// </summary>
public sealed record BusinessIntent<TPayload>(
    string Name,
    TPayload Payload,
    DateTimeOffset RequestedAt,
    string? CorrelationId = null)
    where TPayload : notnull;
