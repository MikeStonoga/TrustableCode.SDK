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
            from: OrderStatus.ShippedWaitingDelivery,
            to: OrderStatus.Delivered,
            currentState: () => order.Status,
            applyState: order.ApplyStatus,
            preconditions:
            [
                new ProofOfDeliveryRequiredPrecondition()
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
