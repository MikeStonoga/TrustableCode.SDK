namespace TrustableCode.SDK.Samples.Ordering;

public sealed record CancelOrderRequirement(
    string Reason,
    string CorrelationId);
