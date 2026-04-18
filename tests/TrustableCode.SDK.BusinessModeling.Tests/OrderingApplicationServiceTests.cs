using TrustableCode.SDK.BusinessModeling.Example.Ordering;
using TrustableCode.SDK.BusinessModeling.Events;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Transactions;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class OrderingApplicationServiceTests
{
    [Fact]
    public async Task PrepareForShippingAsync_should_emit_transition_evidence_and_return_result()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var transactionRunner = new RecordingTransactionRunner();
        var outbox = new InMemoryBusinessEventOutbox();
        var publisher = new InMemoryBusinessEventPublisher();
        var service = new OrderingApplicationService(sink, transactionRunner, outbox, publisher);
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
        Assert.True(transactionRunner.Executed);
        Assert.Single(outbox.Messages);
        Assert.Single(publisher.PublishedMessages);
        Assert.Equal(outbox.Messages[0], publisher.PublishedMessages[0]);
        Assert.Equal(outbox.Messages[0].BusinessEvent.EventId, outbox.Messages[0].EventId);
    }

    [Fact]
    public async Task PrepareForShippingAsync_should_publish_only_after_outbox_transaction_completes()
    {
        var sequence = new List<string>();
        var sink = new InMemoryBusinessEvidenceSink(sequence);
        var transactionRunner = new RecordingTransactionRunner(sequence);
        var outbox = new InMemoryBusinessEventOutbox(sequence);
        var publisher = new InMemoryBusinessEventPublisher(sequence);
        var service = new OrderingApplicationService(sink, transactionRunner, outbox, publisher);
        var order = Order.Create(new CreateOrderRequirement(OrderId.New(), OrderStatus.Paid));

        await service.PrepareForShippingAsync(order, new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-app-2"));

        Assert.Equal(
            ["transaction-begin", "outbox-enqueue", "transaction-commit", "publish", "evidence"],
            sequence);
    }

    [Fact]
    public async Task PrepareForShippingAsync_should_allow_deduplication_by_event_id_on_retry()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var transactionRunner = new RecordingTransactionRunner();
        var outbox = new RetryingBusinessEventOutbox();
        var publisher = new DeduplicatingBusinessEventPublisher();
        var service = new OrderingApplicationService(sink, transactionRunner, outbox, publisher);
        var order = Order.Create(new CreateOrderRequirement(OrderId.New(), OrderStatus.Paid));

        await service.PrepareForShippingAsync(order, new PrepareOrderForShippingRequirement(
            PaymentCaptured: true,
            StockReserved: true,
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-app-3"));

        await publisher.PublishAsync(outbox.RetriedMessages, CancellationToken.None);

        Assert.Single(publisher.AcceptedMessages);
        Assert.Equal(2, publisher.AttemptedMessages.Count);
        Assert.Equal(publisher.AttemptedMessages[0].EventId, publisher.AttemptedMessages[1].EventId);
    }

    private sealed class InMemoryBusinessEvidenceSink : IBusinessEvidenceSink
    {
        public List<IBusinessEvidence> Items { get; } = [];
        private readonly List<string>? _sequence;

        public InMemoryBusinessEvidenceSink(List<string>? sequence = null)
        {
            _sequence = sequence;
        }

        public Task WriteAsync(IBusinessEvidence evidence, CancellationToken cancellationToken = default)
        {
            _sequence?.Add("evidence");
            Items.Add(evidence);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryBusinessEventOutbox(List<string>? sequence = null) : IBusinessEventOutbox
    {
        public List<BusinessEventOutboxMessage> Messages { get; } = [];

        public Task EnqueueAsync(IReadOnlyList<BusinessEventOutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            sequence?.Add("outbox-enqueue");
            Messages.AddRange(messages);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryBusinessEventPublisher(List<string>? sequence = null) : IBusinessEventPublisher
    {
        public List<BusinessEventOutboxMessage> PublishedMessages { get; } = [];

        public Task PublishAsync(IReadOnlyList<BusinessEventOutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            sequence?.Add("publish");
            PublishedMessages.AddRange(messages);
            return Task.CompletedTask;
        }
    }

    private sealed class DeduplicatingBusinessEventPublisher : IBusinessEventPublisher
    {
        private readonly HashSet<Guid> _seenEventIds = [];

        public List<BusinessEventOutboxMessage> AttemptedMessages { get; } = [];

        public List<BusinessEventOutboxMessage> AcceptedMessages { get; } = [];

        public Task PublishAsync(IReadOnlyList<BusinessEventOutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
            {
                AttemptedMessages.Add(message);

                if (_seenEventIds.Add(message.EventId))
                {
                    AcceptedMessages.Add(message);
                }
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RetryingBusinessEventOutbox : IBusinessEventOutbox
    {
        public List<BusinessEventOutboxMessage> StoredMessages { get; } = [];

        public IReadOnlyList<BusinessEventOutboxMessage> RetriedMessages
            => StoredMessages.Select(message => message with { MessageId = Guid.NewGuid() }).ToArray();

        public Task EnqueueAsync(IReadOnlyList<BusinessEventOutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            StoredMessages.AddRange(messages);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTransactionRunner(List<string>? sequence = null) : IBusinessTransactionRunner
    {
        public bool Executed { get; private set; }

        public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            Executed = true;
            sequence?.Add("transaction-begin");
            await operation(cancellationToken);
            sequence?.Add("transaction-commit");
        }
    }
}
