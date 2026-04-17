using TrustableCode.SDK.BusinessModeling.Boundaries;
using TrustableCode.SDK.BusinessModeling.Example.Ordering;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class OrderingApplicationServiceTests
{
    [Fact]
    public async Task PrepareForShippingAsync_should_emit_transition_evidence_and_return_result()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var service = new OrderingApplicationService(sink);
        var order = Order.CreatePaid(OrderId.New());
        var intent = new BusinessIntent<PrepareOrderForShippingRequest>(
            Name: "PrepareOrderForShipping",
            Payload: new PrepareOrderForShippingRequest(PaymentCaptured: true, StockReserved: true),
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-app-1");

        var result = await service.PrepareForShippingAsync(order, intent);

        Assert.Equal(OrderStatus.ReadyForShipping, result.CurrentStatus);
        Assert.Equal("PrepareForShipping", result.CompletedTransition);
        Assert.Single(sink.Items);
        Assert.IsType<BusinessTransitionEvidence<OrderStatus>>(sink.Items[0]);
    }

    private sealed class InMemoryBusinessEvidenceSink : IBusinessEvidenceSink
    {
        public List<IBusinessEvidence> Items { get; } = [];

        public Task WriteAsync(IBusinessEvidence evidence, CancellationToken cancellationToken = default)
        {
            Items.Add(evidence);
            return Task.CompletedTask;
        }
    }
}
