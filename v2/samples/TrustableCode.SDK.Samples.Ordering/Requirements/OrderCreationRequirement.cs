namespace TrustableCode.SDK.Samples.Ordering;

public sealed record OrderCreationRequirement(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<OrderLine> Lines,
    string CorrelationId);
