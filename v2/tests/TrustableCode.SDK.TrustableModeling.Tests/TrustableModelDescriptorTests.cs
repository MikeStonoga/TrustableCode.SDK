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
        Assert.Contains("PlacedAwaitingPayment (initial):", markdown);
        Assert.Contains("Delivered (terminal):", markdown);
        Assert.Contains("## Suggested Reading Order", markdown);
        Assert.Contains("## Expected State Flow", markdown);
        Assert.Contains("PaymentCapturedBeforeShipmentPreparation", markdown);
        Assert.Contains("CreateOrder: External intent -> PlacedAwaitingPayment", markdown);
        Assert.Contains("PrepareForShipping: PaidAwaitingFulfillment -> FulfilledReadyForShipping", markdown);
        Assert.Contains("  - Preconditions: `Payment must be captured.`, `Stock must be reserved.`, `The order must not already be shipped.`", markdown);
        Assert.Contains("CapturePaymentAdmission rejects with `OrderPaymentCaptureRejectedEvidence`", markdown);
        Assert.Contains("Keep side effects idempotent when the descriptor requires an idempotency key.", markdown);
        Assert.Contains("Do not replace named transitions with direct status mutation.", markdown);
        Assert.Contains("Do not create orders by directly constructing arbitrary status.", markdown);
        Assert.Contains("Do not publish fulfillment side effects before the business event is durable.", markdown);
    }

    [Fact]
    public void Ordering_sample_agent_context_export_should_match_the_descriptor()
    {
        var packet = AgentContextPacket.From(OrderFulfillmentTrustableModel.Descriptor);
        var expectedMarkdown = NormalizeMarkdown(packet.ToMarkdown());
        var sampleExportPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "samples",
            "TrustableCode.SDK.Samples.Ordering",
            "agent-context.md"));

        var exportedMarkdown = NormalizeMarkdown(File.ReadAllText(sampleExportPath));

        Assert.Equal(expectedMarkdown, exportedMarkdown);
    }

    private static string NormalizeMarkdown(string markdown)
        => markdown.ReplaceLineEndings("\n").TrimEnd();
}
