using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class OrderMustBeCancellablePrecondition : TransitionPrecondition<OrderStatus, CancelOrderRequirement>
{
    public OrderMustBeCancellablePrecondition()
        : base(
            code: "ShippedOrdersCannotBeCancelled",
            description: "Orders waiting for delivery or already delivered must use return/refund workflows instead.",
            isSatisfied: (state, _) => state is not OrderStatus.ShippedWaitingDelivery and not OrderStatus.Delivered,
            rejectionReason: "Orders waiting for delivery or already delivered cannot be cancelled by this transition.")
    {
    }
}
