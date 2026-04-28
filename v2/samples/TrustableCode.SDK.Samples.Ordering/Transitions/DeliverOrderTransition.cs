using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class DeliverOrderTransition
{
    private readonly GovernedTransition<OrderStatus, DeliverOrderRequirement> _transition;

    public DeliverOrderTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = GovernedTransition<OrderStatus, DeliverOrderRequirement>
            .Create("DeliverOrder")
            .From(OrderStatus.ShippedWaitingDelivery)
            .To(OrderStatus.Delivered)
            .ReadState(() => order.Status)
            .ApplyState(order.ApplyStatus)
            .Require(new ProofOfDeliveryRequiredPrecondition())
            .ProducesEvent("OrderDelivered")
            .ProducesEvidence("OrderDeliveredEvidence")
            .TreatRepetitionAsAlreadyApplied()
            .Build();
    }

    public TransitionExecutionResult<OrderStatus> Execute(DeliverOrderRequirement requirement)
        => _transition.Execute(requirement);
}
