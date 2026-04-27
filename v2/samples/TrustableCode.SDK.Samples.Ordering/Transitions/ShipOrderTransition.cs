using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class ShipOrderTransition
{
    private readonly GovernedTransition<OrderStatus, ShipOrderRequirement> _transition;

    public ShipOrderTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = new GovernedTransition<OrderStatus, ShipOrderRequirement>(
            name: "ShipOrder",
            from: OrderStatus.FulfilledReadyForShipping,
            to: OrderStatus.ShippedWaitingDelivery,
            currentState: () => order.Status,
            applyState: order.ApplyStatus,
            preconditions:
            [
                new CarrierRequiredPrecondition(),
                new TrackingCodeRequiredPrecondition()
            ],
            producedEvents:
            [
                "OrderShipped"
            ],
            producedEvidence:
            [
                "OrderShippedEvidence"
            ],
            repetitionPolicy: TransitionRepetitionPolicy.TreatAsAlreadyApplied);
    }

    public TransitionExecutionResult<OrderStatus> Execute(ShipOrderRequirement requirement)
        => _transition.Execute(requirement);
}
