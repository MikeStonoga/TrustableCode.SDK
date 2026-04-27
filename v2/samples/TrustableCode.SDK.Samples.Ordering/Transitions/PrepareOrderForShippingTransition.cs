using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class PrepareOrderForShippingTransition
{
    private readonly GovernedTransition<OrderStatus, PrepareOrderForShippingRequirement> _transition;

    public PrepareOrderForShippingTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = new GovernedTransition<OrderStatus, PrepareOrderForShippingRequirement>(
            name: "PrepareForShipping",
            from: OrderStatus.PaidAwaitingFulfillment,
            to: OrderStatus.FulfilledReadyForShipping,
            currentState: () => order.Status,
            applyState: order.ApplyStatus,
            invariants: OrderFulfillmentInvariants.PrepareForShipping,
            producedEvents:
            [
                "OrderPreparedForShipping"
            ],
            producedEvidence:
            [
                "OrderPreparedForShippingEvidence"
            ],
            repetitionPolicy: TransitionRepetitionPolicy.TreatAsAlreadyApplied);
    }

    public TransitionExecutionResult<OrderStatus> Execute(PrepareOrderForShippingRequirement requirement)
        => _transition.Execute(requirement);
}
