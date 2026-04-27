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
                new TransitionPrecondition<OrderStatus, ShipOrderRequirement>(
                    code: "CarrierRequired",
                    description: "A shipment must identify the carrier responsible for transport.",
                    isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.Carrier),
                    rejectionReason: "A carrier is required before the order can be shipped."),
                new TransitionPrecondition<OrderStatus, ShipOrderRequirement>(
                    code: "TrackingCodeRequired",
                    description: "A shipment must be traceable after it leaves fulfillment.",
                    isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.TrackingCode),
                    rejectionReason: "A tracking code is required before the order can be shipped.")
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
