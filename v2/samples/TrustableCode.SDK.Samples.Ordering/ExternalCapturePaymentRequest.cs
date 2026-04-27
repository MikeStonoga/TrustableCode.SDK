namespace TrustableCode.SDK.Samples.Ordering;

public sealed record ExternalCapturePaymentRequest(
    bool PaymentCaptured,
    string PaymentReference,
    string? RequestedStatus,
    string CorrelationId);
