namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ExternalDeliverOrderRequest(
    bool ProofOfDeliveryCaptured,
    DateTimeOffset DeliveredAt,
    string? RequestedStatus,
    string CorrelationId);
