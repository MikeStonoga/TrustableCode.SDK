using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class CancelOrderAdmission
{
    public static BusinessAdmission<ExternalCancelOrderRequest, CancelOrderRequirement> Create()
        => BusinessAdmission<ExternalCancelOrderRequest, CancelOrderRequirement>
            .Create("CancelOrderAdmission")
            .Require(
                    code: "BoundaryMustReceiveCancellationIntentNotStatus",
                    description: "The boundary accepts cancellation intent, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may request cancellation, but may not submit an arbitrary target order status.",
                    rejectionEvidenceName: "OrderCancellationRejectedEvidence")
            .Require(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so cancellation can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before cancellation can be admitted.",
                    rejectionEvidenceName: "OrderCancellationRejectedEvidence")
            .AcceptWith(request => new CancelOrderRequirement(
                request.Reason,
                request.CorrelationId))
            .Build();
}
