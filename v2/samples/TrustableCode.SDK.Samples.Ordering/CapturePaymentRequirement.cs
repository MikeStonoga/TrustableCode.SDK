namespace TrustableCode.SDK.Samples.Ordering;

public sealed record CapturePaymentRequirement(
    bool PaymentCaptured,
    string PaymentReference,
    string CorrelationId);
