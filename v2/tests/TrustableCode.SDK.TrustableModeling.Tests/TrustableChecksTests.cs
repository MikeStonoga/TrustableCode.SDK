using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.SideEffects;
using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.SideEffects;
using TrustableCode.SDK.TrustableModeling.Testing;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class TrustableChecksTests
{
    [Fact]
    public void Transition_applied_check_should_validate_state_events_and_evidence()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.PaidAwaitingFulfillment);

        var result = order.PrepareForShipping(scenario.ShippingPreparation(correlationId: "corr-check-1"));

        var check = TrustableChecks.TransitionApplied(
            result,
            OrderStatus.FulfilledReadyForShipping,
            producedEvents: ["OrderPreparedForShipping"],
            producedEvidence: ["OrderPreparedForShippingEvidence"]);

        Assert.True(check.Passed);
    }

    [Fact]
    public void Transition_rejected_check_should_report_missing_expectations()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var order = scenario.RehydratedOrder(OrderStatus.PaidAwaitingFulfillment);

        var result = order.PrepareForShipping(scenario.ShippingPreparation(
            paymentCaptured: false,
            stockReserved: false,
            correlationId: "corr-check-2"));

        var check = TrustableChecks.TransitionRejected(
            result,
            rejectionReasons: ["Payment must be captured before the order can be prepared for shipping."],
            rejectionEvidence: ["MissingEvidence"]);

        Assert.False(check.Passed);
        Assert.Contains("Expected rejection evidence 'MissingEvidence' was not found.", check.Failures);
    }

    [Fact]
    public void Admission_checks_should_validate_accepted_and_rejected_boundaries()
    {
        var accepted = CapturePaymentAdmission.Create().Admit(new ExternalCapturePaymentRequest(
            PaymentCaptured: true,
            PaymentReference: "pay-1",
            RequestedStatus: null,
            CorrelationId: "corr-check-3"));
        var rejected = ShipOrderAdmission.Create().Admit(new ExternalShipOrderRequest(
            Carrier: "DHL",
            TrackingCode: "track-1",
            RequestedStatus: "ShippedWaitingDelivery",
            CorrelationId: "corr-check-4"));

        Assert.True(TrustableChecks.AdmissionAccepted(accepted).Passed);
        Assert.True(TrustableChecks.AdmissionRejected(
            rejected,
            rejectionEvidence: ["OrderShipmentRejectedEvidence"]).Passed);
    }

    [Fact]
    public void Invariant_side_effect_and_evidence_checks_should_validate_common_sdk_outputs()
    {
        var scenario = OrderingScenarioBuilder.Create();
        var context = new TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>(
            TransitionName: "PrepareForShipping",
            CurrentState: OrderStatus.PaidAwaitingFulfillment,
            TargetState: OrderStatus.FulfilledReadyForShipping,
            Input: scenario.ShippingPreparation(
                stockReserved: false,
                correlationId: "corr-check-5"));
        var violation = new InvariantSet<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>>(
            OrderFulfillmentInvariants.PrepareForShipping)
            .FindViolations(context)
            .Single();

        var notifications = new List<FulfillmentNotification>();
        var sideEffect = new NotifyFulfillmentSideEffect(new InMemoryIdempotencyLedger(), notifications);
        var sideEffectResult = sideEffect.Execute(new FulfillmentNotification("order-1", "corr-check-6"));

        Assert.True(TrustableChecks.InvariantViolated(violation, "StockReservedBeforeShipmentPreparation").Passed);
        Assert.True(TrustableChecks.SideEffectStatus(
            sideEffectResult,
            SideEffectExecutionStatus.Executed,
            "NotifyFulfillmentEvidence").Passed);
        Assert.True(TrustableChecks.EvidenceNamed(
            sideEffectResult.Evidence,
            "NotifyFulfillmentEvidence").Passed);
    }
}
