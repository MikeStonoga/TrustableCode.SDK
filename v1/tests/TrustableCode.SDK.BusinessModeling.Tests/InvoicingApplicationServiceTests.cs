using TrustableCode.SDK.BusinessModeling.Events;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Transactions;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class InvoicingApplicationServiceTests
{
    [Fact]
    public async Task RefundAsync_should_persist_publish_and_emit_refund_evidence()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var transactionRunner = new RecordingTransactionRunner();
        var outbox = new InMemoryBusinessEventOutbox();
        var publisher = new InMemoryBusinessEventPublisher();
        var service = new InvoicingApplicationService(sink, transactionRunner, outbox, publisher);
        var invoice = Invoice.Create(new CreateInvoiceRequirement(
            InvoiceId.New(),
            new Money(100m, "USD")));

        var result = await service.RefundAsync(invoice, new RefundInvoiceRequirement(
            RefundAmount: new Money(40m, "USD"),
            Reason: "Customer request",
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-invoice-app-1"));

        Assert.Equal(InvoiceStatus.PartiallyRefunded, result.CurrentStatus);
        Assert.Equal("Refund", result.CompletedTransition);
        Assert.Equal(1, result.EmittedBusinessEvents);
        Assert.Single(outbox.Messages);
        Assert.Single(publisher.PublishedMessages);
        Assert.Contains(sink.Items, evidence => evidence is InvoiceRefundedEvidence);
        Assert.True(transactionRunner.Executed);
    }

    [Fact]
    public async Task ScheduleApprovalCompensationAsync_should_persist_publish_and_emit_all_evidence()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var transactionRunner = new RecordingTransactionRunner();
        var outbox = new InMemoryBusinessEventOutbox();
        var publisher = new InMemoryBusinessEventPublisher();
        var service = new InvoicingApplicationService(sink, transactionRunner, outbox, publisher);
        var approval = InvoiceApproval.Create(new CreateInvoiceApprovalRequirement(
            InvoiceApprovalId.New(),
            DateTimeOffset.UtcNow));

        var result = await service.ScheduleApprovalCompensationAsync(approval, new ScheduleInvoiceApprovalCompensationRequirement(
            DownstreamRejectionReason: "Tax authority rejected the approval downstream.",
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-approval-app-1"));

        Assert.Equal(InvoiceApprovalStatus.ApprovalCompensationPending, result.CurrentStatus);
        Assert.Equal("ScheduleApprovalCompensation", result.CompletedTransition);
        Assert.Equal(1, result.EmittedBusinessEvents);
        Assert.Single(outbox.Messages);
        Assert.Single(publisher.PublishedMessages);
        Assert.Contains(sink.Items, evidence => evidence is InvoiceApprovalCompensationScheduledEvidence);
        Assert.Contains(sink.Items, evidence => evidence is CompensationScheduledEvidence);
        Assert.True(transactionRunner.Executed);
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

    private sealed class InMemoryBusinessEventOutbox : IBusinessEventOutbox
    {
        public List<BusinessEventOutboxMessage> Messages { get; } = [];

        public Task EnqueueAsync(IReadOnlyList<BusinessEventOutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            Messages.AddRange(messages);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryBusinessEventPublisher : IBusinessEventPublisher
    {
        public List<BusinessEventOutboxMessage> PublishedMessages { get; } = [];

        public Task PublishAsync(IReadOnlyList<BusinessEventOutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            PublishedMessages.AddRange(messages);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTransactionRunner : IBusinessTransactionRunner
    {
        public bool Executed { get; private set; }

        public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            Executed = true;
            await operation(cancellationToken);
        }
    }
}
