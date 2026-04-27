namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ExternalCancelOrderRequest(
    string Reason,
    string? RequestedStatus,
    string CorrelationId);
