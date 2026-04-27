using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Admission;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class TrustableAdmissionFlowTests
{
    [Fact]
    public void Execute_transition_should_stop_before_transition_when_admission_rejects_input()
    {
        var transitionWasCalled = false;

        var result = TrustableAdmissionFlow.ExecuteTransition(
            PrepareOrderForShippingAdmission.Create(),
            new ExternalPrepareOrderForShippingRequest(
                PaymentCaptured: true,
                StockReserved: true,
                RequestedStatus: "Delivered",
                CorrelationId: "corr-flow-1"),
            requirement =>
            {
                transitionWasCalled = true;
                return new TransitionExecutionResult<OrderStatus>(
                    "PrepareForShipping",
                    OrderStatus.PaidAwaitingFulfillment,
                    OrderStatus.FulfilledReadyForShipping,
                    TransitionExecutionStatus.Applied);
            });

        Assert.False(result.WasAccepted);
        Assert.False(result.WasApplied);
        Assert.Null(result.Transition);
        Assert.False(transitionWasCalled);
        Assert.Contains(
            "External callers may request shipment preparation, but may not submit an arbitrary target order status.",
            result.RejectionReasons);
    }

    [Fact]
    public void Execute_transition_should_return_transition_result_when_admission_accepts_input()
    {
        var order = Order.Rehydrate(OrderStatus.PaidAwaitingFulfillment);

        var result = TrustableAdmissionFlow.ExecuteTransition(
            PrepareOrderForShippingAdmission.Create(),
            new ExternalPrepareOrderForShippingRequest(
                PaymentCaptured: true,
                StockReserved: true,
                RequestedStatus: "",
                CorrelationId: "corr-flow-2"),
            order.PrepareForShipping);

        Assert.True(result.WasAccepted);
        Assert.True(result.WasApplied);
        Assert.Equal(TransitionExecutionStatus.Applied, result.Transition?.Status);
        Assert.Equal(OrderStatus.FulfilledReadyForShipping, result.Transition?.CurrentState);
    }
}
