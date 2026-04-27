namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ExternalCreateOrderRequest(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<OrderLine> Lines,
    string? RequestedStatus,
    string CorrelationId);
