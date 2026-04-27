using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class PrepareOrderForShippingAdmission
{
    public static BusinessAdmission<ExternalPrepareOrderForShippingRequest, PrepareOrderForShippingRequirement> Create()
        => new(
            name: "PrepareOrderForShippingAdmission",
            rules:
            [
                new AdmissionRule<ExternalPrepareOrderForShippingRequest>(
                    code: "BoundaryMustReceiveIntentNotStatus",
                    description: "The boundary accepts intent to prepare for shipping, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may request shipment preparation, but may not submit an arbitrary target order status."),
                new AdmissionRule<ExternalPrepareOrderForShippingRequest>(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so rejected or accepted admission can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before order preparation can be admitted.")
            ],
            accept: request => new PrepareOrderForShippingRequirement(
                request.PaymentCaptured,
                request.StockReserved,
                request.CorrelationId));
}

