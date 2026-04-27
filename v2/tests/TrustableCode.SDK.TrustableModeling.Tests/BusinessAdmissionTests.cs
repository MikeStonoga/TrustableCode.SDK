using TrustableCode.SDK.Samples.Ordering;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class BusinessAdmissionTests
{
    [Fact]
    public void Prepare_order_for_shipping_admission_should_accept_intent_without_target_status()
    {
        var admission = PrepareOrderForShippingAdmission.Create();

        var result = admission.Admit(new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "",
            CorrelationId: "corr-admission-1"));

        Assert.True(result.WasAccepted);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.PaymentCaptured);
        Assert.True(result.Value.StockReserved);
        Assert.Equal("corr-admission-1", result.Value.CorrelationId);
    }

    [Fact]
    public void Prepare_order_for_shipping_admission_should_reject_arbitrary_status_mutation()
    {
        var admission = PrepareOrderForShippingAdmission.Create();

        var result = admission.Admit(new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "Shipped",
            CorrelationId: "corr-admission-2"));

        Assert.False(result.WasAccepted);
        Assert.Null(result.Value);
        Assert.Contains(
            "External callers may request shipment preparation, but may not submit an arbitrary target order status.",
            result.RejectionReasons);
        Assert.Single(result.RejectionEvidence);
        Assert.Equal("BoundaryMustReceiveIntentNotStatusRejected", result.RejectionEvidence[0].Name);
        Assert.Equal("PrepareOrderForShippingAdmission", result.RejectionEvidence[0].Metadata["admission.name"]);
    }

    [Fact]
    public void Prepare_order_for_shipping_admission_should_reject_missing_correlation()
    {
        var admission = PrepareOrderForShippingAdmission.Create();

        var result = admission.Admit(new ExternalPrepareOrderForShippingRequest(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedStatus: "",
            CorrelationId: ""));

        Assert.False(result.WasAccepted);
        Assert.Contains("A correlation id is required before order preparation can be admitted.", result.RejectionReasons);
        Assert.Single(result.RejectionEvidence);
        Assert.Equal("CorrelationIdRequiredRejected", result.RejectionEvidence[0].Name);
    }
}
