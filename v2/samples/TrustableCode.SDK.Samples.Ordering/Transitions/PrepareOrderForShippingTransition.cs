using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class PrepareOrderForShippingTransition
{
    private readonly GovernedTransition<OrderStatus, PrepareOrderForShippingRequirement> _transition;

    public PrepareOrderForShippingTransition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _transition = GovernedTransition<OrderStatus, PrepareOrderForShippingRequirement>
            .Create("PrepareForShipping")
            .From(OrderStatus.PaidAwaitingFulfillment)
            .To(OrderStatus.FulfilledReadyForShipping)
            .ReadState(() => order.Status)
            .ApplyState(order.ApplyStatus)
            .Preserve(OrderFulfillmentInvariants.PrepareForShipping)
            .ProducesEvent("OrderPreparedForShipping")
            .ProducesEvidence("OrderPreparedForShippingEvidence")
            .TreatRepetitionAsAlreadyApplied()
            .Build();
    }

    public TransitionExecutionResult<OrderStatus> Execute(PrepareOrderForShippingRequirement requirement)
        => _transition.Execute(requirement);
}
