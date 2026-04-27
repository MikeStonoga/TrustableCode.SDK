using TrustableCode.SDK.BusinessModeling.Events;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Transactions;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

/// <summary>
/// Example application service showing how refund and compensation workflows can persist outbox messages transactionally
/// and publish business events only after commit.
/// </summary>
public sealed class InvoicingApplicationService
{
    private readonly IBusinessEvidenceSink _evidenceSink;
    private readonly IBusinessTransactionRunner _transactionRunner;
    private readonly IBusinessEventOutbox _eventOutbox;
    private readonly IBusinessEventPublisher _eventPublisher;

    public InvoicingApplicationService(
        IBusinessEvidenceSink evidenceSink,
        IBusinessTransactionRunner transactionRunner,
        IBusinessEventOutbox eventOutbox,
        IBusinessEventPublisher eventPublisher)
    {
        _evidenceSink = evidenceSink ?? throw new ArgumentNullException(nameof(evidenceSink));
        _transactionRunner = transactionRunner ?? throw new ArgumentNullException(nameof(transactionRunner));
        _eventOutbox = eventOutbox ?? throw new ArgumentNullException(nameof(eventOutbox));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<InvoiceRefundResult> RefundAsync(
        Invoice invoice,
        RefundInvoiceRequirement requirement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(requirement);

        InvoiceRefundedEvidence? transitionEvidence = null;
        IReadOnlyList<BusinessEventOutboxMessage> outboxMessages = [];

        await _transactionRunner.ExecuteAsync(async transactionCancellationToken =>
        {
            transitionEvidence = invoice.Refund(requirement);
            outboxMessages = BusinessEventOutboxBatch.CreateFrom(
                streamName: $"{nameof(Invoice)}-{invoice.Id}",
                source: invoice,
                enqueuedAt: requirement.RequestedAt);

            await _eventOutbox.EnqueueAsync(outboxMessages, transactionCancellationToken);
        }, cancellationToken);

        await _eventPublisher.PublishAsync(outboxMessages, cancellationToken);

        if (transitionEvidence is null)
        {
            throw new InvalidOperationException("Transition evidence should have been produced before commit.");
        }

        foreach (var evidence in invoice.DequeueBusinessEvidence())
        {
            await _evidenceSink.WriteAsync(evidence, cancellationToken);
        }

        return new InvoiceRefundResult(
            invoice.Id,
            invoice.Status,
            transitionEvidence.TransitionName,
            outboxMessages.Count);
    }

    public async Task<InvoiceApprovalCompensationApplicationResult> ScheduleApprovalCompensationAsync(
        InvoiceApproval approval,
        ScheduleInvoiceApprovalCompensationRequirement requirement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approval);
        ArgumentNullException.ThrowIfNull(requirement);

        InvoiceApprovalCompensationScheduledEvidence? transitionEvidence = null;
        IReadOnlyList<BusinessEventOutboxMessage> outboxMessages = [];

        await _transactionRunner.ExecuteAsync(async transactionCancellationToken =>
        {
            var result = approval.ScheduleApprovalCompensation(requirement);
            transitionEvidence = result.TransitionEvidence;
            outboxMessages = BusinessEventOutboxBatch.CreateFrom(
                streamName: $"{nameof(InvoiceApproval)}-{approval.Id}",
                source: approval,
                enqueuedAt: requirement.RequestedAt);

            await _eventOutbox.EnqueueAsync(outboxMessages, transactionCancellationToken);
        }, cancellationToken);

        await _eventPublisher.PublishAsync(outboxMessages, cancellationToken);

        if (transitionEvidence is null)
        {
            throw new InvalidOperationException("Transition evidence should have been produced before commit.");
        }

        foreach (var evidence in approval.DequeueBusinessEvidence())
        {
            await _evidenceSink.WriteAsync(evidence, cancellationToken);
        }

        return new InvoiceApprovalCompensationApplicationResult(
            approval.Id,
            approval.Status,
            transitionEvidence.TransitionName,
            outboxMessages.Count);
    }
}
