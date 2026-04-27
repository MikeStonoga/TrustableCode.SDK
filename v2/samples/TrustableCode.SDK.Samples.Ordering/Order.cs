using TrustableCode.SDK.Samples.Ordering.Transitions;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed class Order
{
    private readonly List<string> _events = [];
    private readonly List<string> _evidence = [];
    private readonly List<BusinessEvidence> _businessEvidence = [];

    internal Order(
        OrderStatus status,
        string orderId = "order-1",
        string customerId = "customer-1",
        IEnumerable<OrderLine>? lines = null)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Lines = lines?.ToArray() ?? [new OrderLine("sku-1", 1)];
        Status = status;
    }

    public static Order Rehydrate(
        OrderStatus status,
        string orderId = "order-1",
        string customerId = "customer-1",
        IEnumerable<OrderLine>? lines = null)
        => new(status, orderId, customerId, lines);

    public string OrderId { get; }

    public string CustomerId { get; }

    public IReadOnlyList<OrderLine> Lines { get; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<string> Events => _events;

    public IReadOnlyList<string> Evidence => _evidence;

    public IReadOnlyList<BusinessEvidence> BusinessEvidence => _businessEvidence;

    internal void RecordCreated(string correlationId)
    {
        _events.Add("OrderCreated");
        _evidence.Add("OrderCreatedEvidence");
        _businessEvidence.Add(new BusinessEvidence(
            name: "OrderCreatedEvidence",
            kind: TrustableCode.SDK.TrustableModeling.Modeling.EvidenceKind.Transition,
            message: "Order created and is awaiting payment.",
            correlationId: correlationId,
            metadata: new Dictionary<string, string>
            {
                ["order.id"] = OrderId,
                ["order.customer_id"] = CustomerId,
                ["order.status"] = Status.ToString()
            }));
    }

    public TransitionExecutionResult<OrderStatus> CapturePayment(CapturePaymentRequirement requirement)
        => Record(new CapturePaymentTransition(this).Execute(requirement), "OrderPaymentCaptureRejectedEvidence");

    public TransitionExecutionResult<OrderStatus> PrepareForShipping(PrepareOrderForShippingRequirement requirement)
        => Record(new PrepareOrderForShippingTransition(this).Execute(requirement), "OrderPreparationRejectedEvidence");

    public TransitionExecutionResult<OrderStatus> Ship(ShipOrderRequirement requirement)
        => Record(new ShipOrderTransition(this).Execute(requirement), "OrderShipmentRejectedEvidence");

    public TransitionExecutionResult<OrderStatus> Deliver(DeliverOrderRequirement requirement)
        => Record(new DeliverOrderTransition(this).Execute(requirement), "OrderDeliveryRejectedEvidence");

    public TransitionExecutionResult<OrderStatus> Cancel(CancelOrderRequirement requirement)
        => Record(new CancelOrderTransition(this).Execute(requirement), "OrderCancellationRejectedEvidence");

    internal void ApplyStatus(OrderStatus status)
        => Status = status;

    private TransitionExecutionResult<OrderStatus> Record(
        TransitionExecutionResult<OrderStatus> result,
        string fallbackRejectionEvidence)
    {
        if (result.Status == TransitionExecutionStatus.Applied)
        {
            _events.AddRange(result.ProducedEvents);
            _evidence.AddRange(result.ProducedEvidence);
        }

        if (result.Status == TransitionExecutionStatus.Rejected)
        {
            _evidence.Add(fallbackRejectionEvidence);
            _businessEvidence.AddRange(result.RejectionEvidence);
        }

        return result;
    }
}
