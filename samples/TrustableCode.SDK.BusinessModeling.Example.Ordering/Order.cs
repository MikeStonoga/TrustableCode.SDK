using TrustableCode.SDK.BusinessModeling;
using TrustableCode.SDK.BusinessModeling.Boundaries;
using TrustableCode.SDK.BusinessModeling.Invariants;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Transitions;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed class Order : AggregateRoot
{
    private readonly List<IBusinessEvidence> _businessEvidence = [];

    private Order(OrderId id, OrderStatus status)
    {
        Id = id;
        Status = status;
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

    public static Order CreatePaid(OrderId id)
        => new(id, OrderStatus.Paid);

    public BusinessTransitionEvidence<OrderStatus> PrepareForShipping(BusinessIntent<PrepareOrderForShippingRequest> intent)
    {
        EnsureAll(notification => notification
            .Collect(new OrderMustBePaidBeforePreparingForShippingRule(Status))
            .Collect(new PaymentMustBeCapturedBeforePreparingOrderForShippingRule(intent.Payload.PaymentCaptured))
            .Collect(new StockMustBeReservedBeforePreparingOrderForShippingRule(intent.Payload.StockReserved)));

        var transition = new NamedBusinessTransition<OrderStatus>(
            name: "PrepareForShipping",
            to: OrderStatus.ReadyForShipping,
                currentStateAccessor: () => Status,
                apply: next => Status = next,
                canTransition: current => current == OrderStatus.Paid,
                exceptionFactory: _ => new OrderMustBePaidBeforePreparingForShippingException());

        var executedTransition = transition.Execute();
        RecordBusinessEvent(new OrderPreparedForShipping(Id, intent.RequestedAt));

        var evidence = new BusinessTransitionEvidence<OrderStatus>(
            ModelName: nameof(Order),
            TransitionName: executedTransition.Name,
            PreviousState: executedTransition.From,
            CurrentState: executedTransition.To,
            CorrelationId: intent.CorrelationId,
            ObservedAt: intent.RequestedAt);

        _businessEvidence.Add(evidence);
        return evidence;
    }

    public BusinessTransitionEvidence<OrderStatus> Cancel(DateTimeOffset requestedAt, string? correlationId = null)
    {
        Ensure(new ShippedOrdersCannotBeCancelledRule(Status));

        var previous = Status;
        Status = OrderStatus.Cancelled;

        var evidence = new BusinessTransitionEvidence<OrderStatus>(
            ModelName: nameof(Order),
            TransitionName: "Cancel",
            PreviousState: previous,
            CurrentState: Status,
            CorrelationId: correlationId,
            ObservedAt: requestedAt);

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
