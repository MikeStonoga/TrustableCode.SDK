using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class CancelOrderTransition
{
    private readonly GovernedTransition<OrderStatus, CancelOrderRequirement> _transition;

    public CancelOrderTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = new GovernedTransition<OrderStatus, CancelOrderRequirement>(
            name: "CancelOrder",
            from: order.Status,
            to: OrderStatus.Cancelled,
            currentState: () => order.Status,
            applyState: order.ApplyStatus,
            preconditions:
            [
                new CancellationReasonRequiredPrecondition(),
                new OrderMustBeCancellablePrecondition()
            ],
            producedEvents:
            [
                "OrderCancelled"
            ],
            producedEvidence:
            [
                "OrderCancelledEvidence"
            ],
            repetitionPolicy: TransitionRepetitionPolicy.TreatAsAlreadyApplied);
    }

    public TransitionExecutionResult<OrderStatus> Execute(CancelOrderRequirement requirement)
        => _transition.Execute(requirement);
}
