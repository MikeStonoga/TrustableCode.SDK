using TrustableCode.SDK.BusinessModeling;
using TrustableCode.SDK.BusinessModeling.Invariants;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Invariants;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed class Order : AggregateRoot
{
    private readonly List<IBusinessEvidence> _businessEvidence = [];

    private Order(CreateOrderRequirement requirement)
    {
        Id = requirement.OrderId;
        Status = requirement.InitialStatus;
    }

    public static BusinessInvariantManifest<OrderInvariant> Invariants { get; } =
        BusinessInvariantManifest<OrderInvariant>.Create(builder => builder
            .Add(OrderInvariant.PaymentMustBeCapturedBeforeShipping, "Payment must be captured before the order can be prepared for shipping.")
            .Add(OrderInvariant.StockMustBeReservedBeforeShipping, "Stock must be reserved before the order can be prepared for shipping.")
            .Add(OrderInvariant.OnlyPaidOrdersCanBePreparedForShipping, "Only paid orders may transition to ReadyForShipping.")
            .Add(OrderInvariant.ShippedOrdersCannotBeCancelled, "Shipped orders cannot be cancelled."));

    public OrderId Id { get; }

    public OrderStatus Status { get; private set; }

    public bool IsPaid => Status == OrderStatus.Paid;

    public bool IsReadyForShipping => Status == OrderStatus.ReadyForShipping;

    public bool IsShipped => Status == OrderStatus.Shipped;

    public IReadOnlyList<IBusinessEvidence> BusinessEvidence => _businessEvidence;

    public static Order Create(CreateOrderRequirement requirement)
        => new(requirement);

    public OrderPreparedForShippingEvidence PrepareForShipping(PrepareOrderForShippingRequirement requirement)
    {
        EnsureAll(notification => notification
            .Collect(new OrderMustBePaidBeforePreparingForShippingRule(Status))
            .Collect(new PaymentMustBeCapturedBeforePreparingOrderForShippingRule(requirement.PaymentCaptured))
            .Collect(new StockMustBeReservedBeforePreparingOrderForShippingRule(requirement.StockReserved)));

        var executedTransition = new PrepareOrderForShippingTransition(
            currentStateAccessor: () => Status,
            apply: next => Status = next)
            .Execute();
        RecordBusinessEvent(new OrderPreparedForShipping(Id, requirement.RequestedAt));

        var evidence = new OrderPreparedForShippingEvidence(
            PreviousState: executedTransition.From,
            CurrentState: executedTransition.To,
            CorrelationId: requirement.CorrelationId,
            ObservedAt: requirement.RequestedAt);

        _businessEvidence.Add(evidence);
        return evidence;
    }

    public OrderCancelledEvidence Cancel(CancelOrderRequirement requirement)
    {
        Ensure(new ShippedOrdersCannotBeCancelledRule(Status));

        var executedTransition = new CancelOrderTransition(
            currentStateAccessor: () => Status,
            apply: next => Status = next)
            .Execute();

        RecordBusinessEvent(new OrderCancelled(Id, requirement.RequestedAt));

        var evidence = new OrderCancelledEvidence(
            PreviousState: executedTransition.From,
            CurrentState: executedTransition.To,
            CorrelationId: requirement.CorrelationId,
            ObservedAt: requirement.RequestedAt);

        _businessEvidence.Add(evidence);
        return evidence;
    }

    public IReadOnlyList<IBusinessEvidence> DequeueBusinessEvidence()
    {
        var pending = _businessEvidence.ToArray();
        _businessEvidence.Clear();
        return pending;
    }
}
