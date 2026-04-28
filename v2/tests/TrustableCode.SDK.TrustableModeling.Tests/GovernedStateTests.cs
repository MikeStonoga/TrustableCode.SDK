using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Transitions;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class GovernedStateTests
{
    [Fact]
    public void Governed_state_should_expose_current_state_and_apply_approved_state()
    {
        var state = GovernedState<OrderStatus>.Create(OrderStatus.PaidAwaitingFulfillment);

        state.ApplyApproved(OrderStatus.FulfilledReadyForShipping);

        Assert.Equal(OrderStatus.FulfilledReadyForShipping, state.Current);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, state.Read());
    }

    [Fact]
    public void Transition_builder_should_use_governed_state_for_read_and_apply()
    {
        var state = GovernedState<OrderStatus>.Create(OrderStatus.FulfilledReadyForShipping);
        var transition = GovernedTransition<OrderStatus, ShipOrderRequirement>
            .Create("ShipOrder")
            .From(OrderStatus.FulfilledReadyForShipping)
            .To(OrderStatus.ShippedWaitingDelivery)
            .State(state)
            .Require(new CarrierRequiredPrecondition())
            .Require(new TrackingCodeRequiredPrecondition())
            .Build();

        var result = transition.Execute(new ShipOrderRequirement(
            Carrier: "DHL",
            TrackingCode: "track-1",
            CorrelationId: "corr-governed-state-1"));

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.Equal(OrderStatus.ShippedWaitingDelivery, state.Current);
    }
}
