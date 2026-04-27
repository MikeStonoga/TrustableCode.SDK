using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class ShipOrderAdmission
{
    public static BusinessAdmission<ExternalShipOrderRequest, ShipOrderRequirement> Create()
        => new(
            name: "ShipOrderAdmission",
            rules:
            [
                new AdmissionRule<ExternalShipOrderRequest>(
                    code: "BoundaryMustReceiveShipmentIntentNotStatus",
                    description: "The boundary accepts shipment facts, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may confirm shipment, but may not submit an arbitrary target order status.",
                    rejectionEvidenceName: "OrderShipmentRejectedEvidence"),
                new AdmissionRule<ExternalShipOrderRequest>(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so shipment can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before shipment can be admitted.",
                    rejectionEvidenceName: "OrderShipmentRejectedEvidence")
            ],
            accept: request => new ShipOrderRequirement(
                request.Carrier,
                request.TrackingCode,
                request.CorrelationId));
}
