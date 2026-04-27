using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class CapturePaymentAdmission
{
    public static BusinessAdmission<ExternalCapturePaymentRequest, CapturePaymentRequirement> Create()
        => new(
            name: "CapturePaymentAdmission",
            rules:
            [
                new AdmissionRule<ExternalCapturePaymentRequest>(
                    code: "BoundaryMustReceivePaymentIntentNotStatus",
                    description: "The boundary accepts payment capture facts, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may confirm payment capture, but may not submit an arbitrary target order status.",
                    rejectionEvidenceName: "OrderPaymentCaptureRejectedEvidence"),
                new AdmissionRule<ExternalCapturePaymentRequest>(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so payment capture can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before payment capture can be admitted.",
                    rejectionEvidenceName: "OrderPaymentCaptureRejectedEvidence")
            ],
            accept: request => new CapturePaymentRequirement(
                request.PaymentCaptured,
                request.PaymentReference,
                request.CorrelationId));
}
