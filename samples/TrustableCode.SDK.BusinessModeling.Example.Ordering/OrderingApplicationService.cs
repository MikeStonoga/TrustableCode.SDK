using TrustableCode.SDK.BusinessModeling.Events;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Transactions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

/// <summary>
/// Example application service showing how an application layer can orchestrate a business model, persist outbox messages transactionally,
/// and publish business events after commit.
/// </summary>
public sealed class OrderingApplicationService
{
    private readonly IBusinessEvidenceSink _evidenceSink;
    private readonly IBusinessTransactionRunner _transactionRunner;
    private readonly IBusinessEventOutbox _eventOutbox;
    private readonly IBusinessEventPublisher _eventPublisher;

    public OrderingApplicationService(
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

    public async Task<OrderPreparationResult> PrepareForShippingAsync(
        Order order,
        PrepareOrderForShippingRequirement requirement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(requirement);

        OrderPreparedForShippingEvidence? transitionEvidence = null;
        IReadOnlyList<BusinessEventOutboxMessage> outboxMessages = [];

        await _transactionRunner.ExecuteAsync(async transactionCancellationToken =>
        {
            transitionEvidence = order.PrepareForShipping(requirement);
            outboxMessages = BusinessEventOutboxBatch.CreateFrom(
                streamName: $"{nameof(Order)}-{order.Id}",
                source: order,
                enqueuedAt: requirement.RequestedAt);

            await _eventOutbox.EnqueueAsync(outboxMessages, transactionCancellationToken);
        }, cancellationToken);

        await _eventPublisher.PublishAsync(outboxMessages, cancellationToken);

        if (transitionEvidence is null)
        {
            throw new InvalidOperationException("Transition evidence should have been produced before commit.");
        }

        await _evidenceSink.WriteAsync(transitionEvidence, cancellationToken);

        foreach (var evidence in order.DequeueBusinessEvidence())
        {
            if (!ReferenceEquals(evidence, transitionEvidence))
            {
                await _evidenceSink.WriteAsync(evidence, cancellationToken);
            }
        }

        return new OrderPreparationResult(
            order.Id,
            order.Status,
            transitionEvidence.TransitionName,
            outboxMessages.Count);
    }
}
