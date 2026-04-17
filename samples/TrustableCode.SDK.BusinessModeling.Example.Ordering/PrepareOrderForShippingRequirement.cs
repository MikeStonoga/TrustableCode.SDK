namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record PrepareOrderForShippingRequirement(
    bool PaymentCaptured,
    bool StockReserved,
    DateTimeOffset RequestedAt,
    string? CorrelationId = null);
