namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ShipOrderRequirement(
    string Carrier,
    string TrackingCode,
    string CorrelationId);
