using TrustableCode.SDK.BusinessModeling.Boundaries;
using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Ordering;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class OrderingSampleTests
{
    [Fact]
    public void PrepareForShipping_should_transition_record_event_and_emit_evidence()
    {
        var order = Order.CreatePaid(OrderId.New());
        var intent = new BusinessIntent<PrepareOrderForShippingRequest>(
            Name: "PrepareOrderForShipping",
            Payload: new PrepareOrderForShippingRequest(PaymentCaptured: true, StockReserved: true),
            RequestedAt: new DateTimeOffset(2026, 4, 17, 21, 0, 0, TimeSpan.Zero),
            CorrelationId: "corr-order-1");

        var evidence = order.PrepareForShipping(intent);
        var businessEvents = order.DequeueBusinessEvents();
        var drainedEvidence = order.DequeueBusinessEvidence();

        Assert.True(order.IsReadyForShipping);
        Assert.Single(businessEvents);
        Assert.Single(drainedEvidence);
        Assert.Equal("PrepareForShipping", evidence.TransitionName);
        Assert.Equal(OrderStatus.Paid, evidence.PreviousState);
        Assert.Equal(OrderStatus.ReadyForShipping, evidence.CurrentState);
    }

    [Fact]
    public void PrepareForShipping_should_fail_when_payment_or_stock_are_not_ready()
    {
        var order = Order.CreatePaid(OrderId.New());
        var intent = new BusinessIntent<PrepareOrderForShippingRequest>(
            Name: "PrepareOrderForShipping",
            Payload: new PrepareOrderForShippingRequest(PaymentCaptured: false, StockReserved: false),
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-order-2");

        var exception = Assert.Throws<AggregatedBusinessRuleViolationException>(() => order.PrepareForShipping(intent));

        Assert.Equal(2, exception.Violations.Count);
        Assert.Contains("Payment must be captured before the order can be prepared for shipping.", exception.Violations);
        Assert.Contains("Stock must be reserved before the order can be prepared for shipping.", exception.Violations);
    }

    [Fact]
    public void Order_manifest_should_make_invariants_discoverable()
    {
        Assert.Equal(
            "Only paid orders may transition to ReadyForShipping.",
            Order.Invariants[OrderInvariant.OnlyPaidOrdersCanBePreparedForShipping]);
    }
}
