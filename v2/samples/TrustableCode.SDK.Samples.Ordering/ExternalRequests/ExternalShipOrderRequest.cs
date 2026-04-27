namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ExternalShipOrderRequest(
    string Carrier,
    string TrackingCode,
    string? RequestedStatus,
    string CorrelationId);
