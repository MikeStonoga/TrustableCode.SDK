namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ExternalPrepareOrderForShippingRequest(
    bool PaymentCaptured,
    bool StockReserved,
    string RequestedStatus,
    string CorrelationId);

