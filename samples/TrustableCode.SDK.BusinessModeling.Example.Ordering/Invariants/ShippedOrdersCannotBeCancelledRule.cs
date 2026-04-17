using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Invariants;

public sealed class ShippedOrdersCannotBeCancelledRule(OrderStatus currentStatus) : IBusinessInvariantRule
{
    public string Description => "Orders that have already been shipped cannot be cancelled.";

    public void EnsureIsPreserved()
    {
        if (currentStatus == OrderStatus.Shipped)
        {
            throw new ShippedOrdersCannotBeCancelledException();
        }
    }
}
