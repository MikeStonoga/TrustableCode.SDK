using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Transitions;

public sealed class PrepareOrderForShippingTransition(
    Func<OrderStatus> currentStateAccessor,
    Action<OrderStatus> apply)
    : NamedBusinessTransition<OrderStatus>(
        name: "PrepareForShipping",
        to: OrderStatus.ReadyForShipping,
        currentStateAccessor: currentStateAccessor,
        apply: apply)
{
    protected override bool CanTransitionFrom(OrderStatus currentState)
        => currentState == OrderStatus.Paid;

    protected override BusinessRuleViolationException CreateException(OrderStatus currentState)
        => new OrderMustBePaidBeforePreparingForShippingException();
}
