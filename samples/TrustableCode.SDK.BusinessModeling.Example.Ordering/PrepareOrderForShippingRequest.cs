namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record PrepareOrderForShippingRequest(
    bool PaymentCaptured,
    bool StockReserved);
