using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class DeliverOrderTransition
{
    private readonly GovernedTransition<OrderStatus, DeliverOrderRequirement> _transition;

    public DeliverOrderTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = new GovernedTransition<OrderStatus, DeliverOrderRequirement>(
            name: "DeliverOrder",
            from: OrderStatus.Shipped,
            to: OrderStatus.Delivered,
            currentState: () => order.Status,
            applyState: order.ApplyStatus,
            preconditions:
            [
                new TransitionPrecondition<OrderStatus, DeliverOrderRequirement>(
                    code: "ProofOfDeliveryRequired",
                    description: "Delivery must be backed by carrier or customer confirmation.",
                    isSatisfied: (_, requirement) => requirement.ProofOfDeliveryCaptured,
                    rejectionReason: "Proof of delivery is required before the order can be delivered.")
            ],
            producedEvents:
            [
                "OrderDelivered"
            ],
            producedEvidence:
            [
                "OrderDeliveredEvidence"
            ],
            repetitionPolicy: TransitionRepetitionPolicy.TreatAsAlreadyApplied);
    }

    public TransitionExecutionResult<OrderStatus> Execute(DeliverOrderRequirement requirement)
        => _transition.Execute(requirement);
}
