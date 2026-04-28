using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class CapturePaymentAdmission
{
    public static BusinessAdmission<ExternalCapturePaymentRequest, CapturePaymentRequirement> Create()
        => BusinessAdmission<ExternalCapturePaymentRequest, CapturePaymentRequirement>
            .Create("CapturePaymentAdmission")
            .Require(
                    code: "BoundaryMustReceivePaymentIntentNotStatus",
                    description: "The boundary accepts payment capture facts, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may confirm payment capture, but may not submit an arbitrary target order status.",
                    rejectionEvidenceName: "OrderPaymentCaptureRejectedEvidence")
            .Require(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so payment capture can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before payment capture can be admitted.",
                    rejectionEvidenceName: "OrderPaymentCaptureRejectedEvidence")
            .AcceptWith(request => new CapturePaymentRequirement(
                request.PaymentCaptured,
                request.PaymentReference,
                request.CorrelationId))
            .Build();
}
