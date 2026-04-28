using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class DeliverOrderAdmission
{
    public static BusinessAdmission<ExternalDeliverOrderRequest, DeliverOrderRequirement> Create()
        => BusinessAdmission<ExternalDeliverOrderRequest, DeliverOrderRequirement>
            .Create("DeliverOrderAdmission")
            .Require(
                    code: "BoundaryMustReceiveDeliveryIntentNotStatus",
                    description: "The boundary accepts delivery evidence, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may confirm delivery, but may not submit an arbitrary target order status.",
                    rejectionEvidenceName: "OrderDeliveryRejectedEvidence")
            .Require(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so delivery can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before delivery can be admitted.",
                    rejectionEvidenceName: "OrderDeliveryRejectedEvidence")
            .AcceptWith(request => new DeliverOrderRequirement(
                request.ProofOfDeliveryCaptured,
                request.DeliveredAt,
                request.CorrelationId))
            .Build();
}
