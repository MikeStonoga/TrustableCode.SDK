using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Transitions;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class GovernedTransitionBuilderTests
{
    [Fact]
    public void Build_should_create_transition_from_declared_steps()
    {
        var state = GovernedState<OrderStatus>.Create(OrderStatus.FulfilledReadyForShipping);

        var transition = GovernedTransition<OrderStatus, ShipOrderRequirement>
            .Create("ShipOrder")
            .From(OrderStatus.FulfilledReadyForShipping)
            .To(OrderStatus.ShippedWaitingDelivery)
            .State(state)
            .Require(new CarrierRequiredPrecondition())
            .ProducesEvent("OrderShipped")
            .ProducesEvidence("OrderShippedEvidence")
            .TreatRepetitionAsAlreadyApplied()
            .Build();

        var result = transition.Execute(new ShipOrderRequirement(
            Carrier: "DHL",
            TrackingCode: "track-1",
            CorrelationId: "corr-builder-1"));

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.Equal(OrderStatus.ShippedWaitingDelivery, state.Current);
        Assert.Contains("OrderShipped", result.ProducedEvents);
        Assert.Contains("OrderShippedEvidence", result.ProducedEvidence);
    }

    [Fact]
    public void Build_should_require_state_reader()
    {
        var builder = GovernedTransition<OrderStatus, ShipOrderRequirement>
            .Create("ShipOrder")
            .From(OrderStatus.FulfilledReadyForShipping)
            .To(OrderStatus.ShippedWaitingDelivery)
            .ApplyState(_ => { });

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("must declare how to read current state", exception.Message);
    }

    [Fact]
    public void Build_should_require_state_applier()
    {
        var builder = GovernedTransition<OrderStatus, ShipOrderRequirement>
            .Create("ShipOrder")
            .From(OrderStatus.FulfilledReadyForShipping)
            .To(OrderStatus.ShippedWaitingDelivery)
            .ReadState(() => OrderStatus.FulfilledReadyForShipping);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("must declare how to apply approved state", exception.Message);
    }
}
