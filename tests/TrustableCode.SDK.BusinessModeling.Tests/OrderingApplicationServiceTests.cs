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
        var order = Order.Create(new CreateOrderRequirement(OrderId.New(), OrderStatus.Paid));
        var requirement = new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-app-1");

        var result = await service.PrepareForShippingAsync(order, requirement);

        Assert.Equal(OrderStatus.ReadyForShipping, result.CurrentStatus);
        Assert.Equal("PrepareForShipping", result.CompletedTransition);
        Assert.Equal(1, result.EmittedBusinessEvents);
        Assert.Single(sink.Items);
        Assert.IsType<OrderPreparedForShippingEvidence>(sink.Items[0]);
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
