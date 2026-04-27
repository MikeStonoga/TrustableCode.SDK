using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Transitions;
using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class BusinessInvariantTests
{
    [Fact]
    public void Invariant_set_should_report_only_violated_business_truths()
    {
        var context = new TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>(
            TransitionName: "PrepareForShipping",
            CurrentState: OrderStatus.PaidAwaitingFulfillment,
            TargetState: OrderStatus.FulfilledReadyForShipping,
            Input: new PrepareOrderForShippingRequirement(
                PaymentCaptured: true,
                StockReserved: false,
                CorrelationId: "corr-invariant-1"));

        var violations = new InvariantSet<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>>(
            OrderFulfillmentInvariants.PrepareForShipping)
            .FindViolations(context);

        Assert.Single(violations);
        Assert.Equal("StockReservedBeforeShipmentPreparation", violations[0].Code);
        Assert.Equal("Stock must be reserved before the order can be prepared for shipping.", violations[0].Message);

        var evidence = violations[0].ToEvidence("corr-invariant-1");
        Assert.Equal("StockReservedBeforeShipmentPreparationViolation", evidence.Name);
        Assert.Equal("corr-invariant-1", evidence.CorrelationId);
        Assert.Equal("StockReservedBeforeShipmentPreparation", evidence.Metadata["invariant.code"]);
    }

    [Fact]
    public void Specialized_transition_precondition_should_evaluate_as_business_invariant_rule()
    {
        var precondition = new PaymentMustBeCapturedPrecondition();
        var context = new TransitionContext<OrderStatus, CapturePaymentRequirement>(
            TransitionName: "CapturePayment",
            CurrentState: OrderStatus.PlacedAwaitingPayment,
            TargetState: OrderStatus.PaidAwaitingFulfillment,
            Input: new CapturePaymentRequirement(
                PaymentCaptured: false,
                PaymentReference: "pay-1",
                CorrelationId: "corr-invariant-2"));

        var evaluation = precondition.Evaluate(context);

        Assert.False(evaluation.IsPreserved);
        Assert.Equal("PaymentMustBeCaptured", evaluation.Code);
        Assert.Equal("Payment must be captured before the order can await fulfillment.", evaluation.Message);
    }
}
