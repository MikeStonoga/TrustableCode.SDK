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
                new TransitionPrecondition<OrderStatus, CancelOrderRequirement>(
                    code: "ReasonRequired",
                    description: "Cancellation must be auditable.",
                    isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.Reason),
                    rejectionReason: "A cancellation reason is required."),
                new TransitionPrecondition<OrderStatus, CancelOrderRequirement>(
                    code: "ShippedOrdersCannotBeCancelled",
                    description: "Shipped or delivered orders must use return/refund workflows instead.",
                    isSatisfied: (state, _) => state is not OrderStatus.Shipped and not OrderStatus.Delivered,
                    rejectionReason: "Shipped or delivered orders cannot be cancelled by this transition.")
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
