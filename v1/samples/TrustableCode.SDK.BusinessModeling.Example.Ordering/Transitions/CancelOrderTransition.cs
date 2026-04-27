using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Transitions;

public sealed class CancelOrderTransition(
    Func<OrderStatus> currentStateAccessor,
    Action<OrderStatus> apply)
    : NamedBusinessTransition<OrderStatus>(
        name: "Cancel",
        to: OrderStatus.Cancelled,
        currentStateAccessor: currentStateAccessor,
        apply: apply)
{
    protected override bool CanTransitionFrom(OrderStatus currentState)
        => currentState != OrderStatus.Shipped;

    protected override BusinessRuleViolationException CreateException(OrderStatus currentState)
        => new ShippedOrdersCannotBeCancelledException();
}
