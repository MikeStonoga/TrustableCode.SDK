using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class CancelOrderTransition
{
    private readonly GovernedTransition<OrderStatus, CancelOrderRequirement> _transition;

    public CancelOrderTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = GovernedTransition<OrderStatus, CancelOrderRequirement>
            .Create("CancelOrder")
            .From(order.Status)
            .To(OrderStatus.Cancelled)
            .State(order.StatusState)
            .Require(new CancellationReasonRequiredPrecondition())
            .Require(new OrderMustBeCancellablePrecondition())
            .ProducesEvent("OrderCancelled")
            .ProducesEvidence("OrderCancelledEvidence")
            .TreatRepetitionAsAlreadyApplied()
            .Build();
    }

    public TransitionExecutionResult<OrderStatus> Execute(CancelOrderRequirement requirement)
        => _transition.Execute(requirement);
}
