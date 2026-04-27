using TrustableCode.SDK.Samples.Ordering.Transitions;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed class Order
{
    private readonly List<string> _events = [];
    private readonly List<string> _evidence = [];

    public Order(OrderStatus status)
    {
        Status = status;
    }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<string> Events => _events;

    public IReadOnlyList<string> Evidence => _evidence;

    public TransitionExecutionResult<OrderStatus> PrepareForShipping(PrepareOrderForShippingRequirement requirement)
    {
        var result = new PrepareOrderForShippingTransition(this).Execute(requirement);
        if (result.Status == TransitionExecutionStatus.Applied)
        {
            _events.AddRange(result.ProducedEvents);
            _evidence.AddRange(result.ProducedEvidence);
        }

        if (result.Status == TransitionExecutionStatus.Rejected)
        {
            _evidence.Add("OrderPreparationRejectedEvidence");
        }

        return result;
    }

    internal void ApplyStatus(OrderStatus status)
        => Status = status;
}
