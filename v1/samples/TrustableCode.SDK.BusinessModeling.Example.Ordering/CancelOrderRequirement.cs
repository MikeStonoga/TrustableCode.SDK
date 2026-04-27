namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record CancelOrderRequirement(
    DateTimeOffset RequestedAt,
    string? CorrelationId = null);
