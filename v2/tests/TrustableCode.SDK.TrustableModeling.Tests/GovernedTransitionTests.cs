using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class GovernedTransitionTests
{
    [Fact]
    public void Prepare_for_shipping_should_apply_state_and_record_declared_outputs()
    {
        var order = new Order(OrderStatus.Paid);

        var result = order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            CorrelationId: "corr-1"));

        Assert.Equal(TransitionExecutionStatus.Applied, result.Status);
        Assert.True(result.WasApplied);
        Assert.Equal(OrderStatus.Paid, result.PreviousState);
        Assert.Equal(OrderStatus.ReadyForShipping, result.CurrentState);
        Assert.Equal(OrderStatus.ReadyForShipping, order.Status);
        Assert.Contains("OrderPreparedForShipping", order.Events);
        Assert.Contains("OrderPreparedForShippingEvidence", order.Evidence);
    }

    [Fact]
    public void Prepare_for_shipping_should_reject_when_preconditions_are_not_satisfied()
    {
        var order = new Order(OrderStatus.Paid);

        var result = order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: false,
            StockReserved: false,
            CorrelationId: "corr-2"));

        Assert.Equal(TransitionExecutionStatus.Rejected, result.Status);
        Assert.False(result.WasApplied);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(2, result.RejectionReasons.Count);
        Assert.Contains("Payment must be captured before the order can be prepared for shipping.", result.RejectionReasons);
        Assert.Contains("Stock must be reserved before the order can be prepared for shipping.", result.RejectionReasons);
        Assert.Contains("OrderPreparationRejectedEvidence", order.Evidence);
        Assert.Empty(order.Events);
    }

    [Fact]
    public void Prepare_for_shipping_should_be_idempotent_when_already_ready_for_shipping()
    {
        var order = new Order(OrderStatus.ReadyForShipping);

        var result = order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            CorrelationId: "corr-3"));

        Assert.Equal(TransitionExecutionStatus.AlreadyApplied, result.Status);
        Assert.True(result.WasApplied);
        Assert.Equal(OrderStatus.ReadyForShipping, result.CurrentState);
        Assert.Empty(order.Events);
    }
}

