using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class ShipOrderTransition
{
    private readonly GovernedTransition<OrderStatus, ShipOrderRequirement> _transition;

    public ShipOrderTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = GovernedTransition<OrderStatus, ShipOrderRequirement>
            .Create("ShipOrder")
            .From(OrderStatus.FulfilledReadyForShipping)
            .To(OrderStatus.ShippedWaitingDelivery)
            .ReadState(() => order.Status)
            .ApplyState(order.ApplyStatus)
            .Require(new CarrierRequiredPrecondition())
            .Require(new TrackingCodeRequiredPrecondition())
            .ProducesEvent("OrderShipped")
            .ProducesEvidence("OrderShippedEvidence")
            .TreatRepetitionAsAlreadyApplied()
            .Build();
    }

    public TransitionExecutionResult<OrderStatus> Execute(ShipOrderRequirement requirement)
        => _transition.Execute(requirement);
}
