namespace TrustableCode.SDK.Samples.Ordering;

public sealed record PrepareOrderForShippingRequirement(
    bool PaymentCaptured,
    bool StockReserved,
    string CorrelationId);

