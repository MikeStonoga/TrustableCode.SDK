namespace TrustableCode.SDK.Samples.Ordering;

public sealed record DeliverOrderRequirement(
    bool ProofOfDeliveryCaptured,
    DateTimeOffset DeliveredAt,
    string CorrelationId);
