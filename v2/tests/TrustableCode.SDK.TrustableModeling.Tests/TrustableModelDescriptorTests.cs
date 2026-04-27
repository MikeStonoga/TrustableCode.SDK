using TrustableCode.SDK.TrustableModeling.AgentContext;
using TrustableCode.SDK.TrustableModeling.Modeling;
using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class TrustableModelDescriptorTests
{
    [Fact]
    public void Create_should_describe_the_semantic_safety_envelope()
    {
        var descriptor = OrderFulfillmentTrustableModel.Descriptor;

        Assert.Equal("Order Fulfillment", descriptor.Name);
        Assert.Equal("Order.Status", descriptor.StateModel.AuthoritativeState);
        Assert.Equal(6, descriptor.Transitions.Count);
        Assert.Equal(4, descriptor.Invariants.Count);
        Assert.Equal(6, descriptor.Boundaries.Count);
        Assert.Equal(2, descriptor.SideEffects.Count);
        Assert.Equal(11, descriptor.Evidence.Count);
    }

    [Fact]
    public void Create_should_require_an_explicit_state_model()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            TrustableModelDescriptor.Create(builder => builder
                .Describe("Order Fulfillment", "Protects order movement toward shipment.")));

        Assert.Contains("explicit state model", exception.Message);
    }

    [Fact]
    public void Agent_context_should_render_model_meaning_for_agents_and_reviewers()
    {
        var packet = AgentContextPacket.From(OrderFulfillmentTrustableModel.Descriptor);

        var markdown = packet.ToMarkdown();

        Assert.Contains("# Trustable Context: Order Fulfillment", markdown);
        Assert.Contains("PaymentCapturedBeforeShipmentPreparation", markdown);
        Assert.Contains("CreateOrder: External intent -> PlacedAwaitingPayment", markdown);
        Assert.Contains("PrepareForShipping: PaidAwaitingFulfillment -> FulfilledReadyForShipping", markdown);
        Assert.Contains("Do not replace named transitions with direct status mutation.", markdown);
        Assert.Contains("Do not create orders by directly constructing arbitrary status.", markdown);
        Assert.Contains("Do not publish fulfillment side effects before the business event is durable.", markdown);
    }
}
