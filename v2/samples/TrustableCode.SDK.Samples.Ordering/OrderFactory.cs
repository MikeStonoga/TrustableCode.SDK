using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.Samples.Ordering;

public static class OrderFactory
{
    public static AdmissionResult<Order> Create(ExternalCreateOrderRequest request)
        => CreationAdmission().Admit(request);

    private static BusinessAdmission<ExternalCreateOrderRequest, Order> CreationAdmission()
        => new(
            name: "CreateOrderAdmission",
            rules:
            [
                new AdmissionRule<ExternalCreateOrderRequest>(
                    code: "OrderIdRequired",
                    description: "An order must have a stable identity before it enters the lifecycle.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.OrderId),
                    rejectionReason: "An order id is required before an order can be created.",
                    rejectionEvidenceName: "OrderCreationRejectedEvidence"),
                new AdmissionRule<ExternalCreateOrderRequest>(
                    code: "CustomerIdRequired",
                    description: "An order must belong to a known customer.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CustomerId),
                    rejectionReason: "A customer id is required before an order can be created.",
                    rejectionEvidenceName: "OrderCreationRejectedEvidence"),
                new AdmissionRule<ExternalCreateOrderRequest>(
                    code: "AtLeastOneLineRequired",
                    description: "An order without lines is not business-meaningful.",
                    isSatisfied: request => request.Lines.Count > 0,
                    rejectionReason: "At least one order line is required before an order can be created.",
                    rejectionEvidenceName: "OrderCreationRejectedEvidence"),
                new AdmissionRule<ExternalCreateOrderRequest>(
                    code: "QuantitiesMustBePositive",
                    description: "Every order line must request a positive quantity.",
                    isSatisfied: request => request.Lines.All(line => line.Quantity > 0),
                    rejectionReason: "Order line quantities must be positive.",
                    rejectionEvidenceName: "OrderCreationRejectedEvidence"),
                new AdmissionRule<ExternalCreateOrderRequest>(
                    code: "BoundaryMustReceiveCreationIntentNotStatus",
                    description: "The creation boundary accepts order creation intent, not direct status mutation.",
                    isSatisfied: request => string.IsNullOrWhiteSpace(request.RequestedStatus),
                    rejectionReason: "External callers may create an order, but may not submit an arbitrary initial order status.",
                    rejectionEvidenceName: "OrderCreationRejectedEvidence"),
                new AdmissionRule<ExternalCreateOrderRequest>(
                    code: "CorrelationIdRequired",
                    description: "A correlation id is required so order creation can be traced.",
                    isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
                    rejectionReason: "A correlation id is required before an order can be created.",
                    rejectionEvidenceName: "OrderCreationRejectedEvidence")
            ],
            accept: request =>
            {
                var order = new Order(
                    OrderStatus.AwaitingPayment,
                    request.OrderId,
                    request.CustomerId,
                    request.Lines);

                order.RecordCreated(request.CorrelationId);
                return order;
            });
}
