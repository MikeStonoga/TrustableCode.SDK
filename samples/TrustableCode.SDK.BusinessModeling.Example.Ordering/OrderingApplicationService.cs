using TrustableCode.SDK.BusinessModeling.Boundaries;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

/// <summary>
/// Example application service showing how an application layer can orchestrate a business model and emit evidence.
/// </summary>
public sealed class OrderingApplicationService
{
    private readonly IBusinessEvidenceSink _evidenceSink;

    public OrderingApplicationService(IBusinessEvidenceSink evidenceSink)
    {
        _evidenceSink = evidenceSink ?? throw new ArgumentNullException(nameof(evidenceSink));
    }

    public async Task<OrderPreparationResult> PrepareForShippingAsync(
        Order order,
        BusinessIntent<PrepareOrderForShippingRequest> intent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(intent);

        var transitionEvidence = order.PrepareForShipping(intent);

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
            transitionEvidence.TransitionName);
    }
}
