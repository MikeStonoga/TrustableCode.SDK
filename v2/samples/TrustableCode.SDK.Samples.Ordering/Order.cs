using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed class Order
{
    private readonly List<string> _events = [];
    private readonly List<string> _evidence = [];

    public Order(OrderStatus status)
    {
        Status = status;
    }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<string> Events => _events;

    public IReadOnlyList<string> Evidence => _evidence;

    public TransitionExecutionResult<OrderStatus> PrepareForShipping(PrepareOrderForShippingRequirement requirement)
    {
        var transition = new GovernedTransition<OrderStatus, PrepareOrderForShippingRequirement>(
            name: "PrepareForShipping",
            from: OrderStatus.Paid,
            to: OrderStatus.ReadyForShipping,
            currentState: () => Status,
            applyState: next => Status = next,
            preconditions:
            [
                new TransitionPrecondition<OrderStatus, PrepareOrderForShippingRequirement>(
                    code: "PaymentCapturedBeforeShipmentPreparation",
                    description: "Payment must be captured before shipping preparation.",
                    isSatisfied: (_, context) => context.PaymentCaptured,
                    rejectionReason: "Payment must be captured before the order can be prepared for shipping."),
                new TransitionPrecondition<OrderStatus, PrepareOrderForShippingRequirement>(
                    code: "StockReservedBeforeShipmentPreparation",
                    description: "Stock must be reserved before shipping preparation.",
                    isSatisfied: (_, context) => context.StockReserved,
                    rejectionReason: "Stock must be reserved before the order can be prepared for shipping.")
            ],
            producedEvents:
            [
                "OrderPreparedForShipping"
            ],
            producedEvidence:
            [
                "OrderPreparedForShippingEvidence"
            ],
            repetitionPolicy: TransitionRepetitionPolicy.TreatAsAlreadyApplied);

        var result = transition.Execute(requirement);
        if (result.Status == TransitionExecutionStatus.Applied)
        {
            _events.AddRange(result.ProducedEvents);
            _evidence.AddRange(result.ProducedEvidence);
        }

        if (result.Status == TransitionExecutionStatus.Rejected)
        {
            _evidence.Add("OrderPreparationRejectedEvidence");
        }

        return result;
    }
}

