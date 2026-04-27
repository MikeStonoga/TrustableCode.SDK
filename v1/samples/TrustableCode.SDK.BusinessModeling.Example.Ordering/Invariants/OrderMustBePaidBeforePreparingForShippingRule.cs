using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Invariants;

public sealed class OrderMustBePaidBeforePreparingForShippingRule(OrderStatus currentStatus) : IBusinessInvariantRule
{
    public string Description => "Only paid orders may transition to ReadyForShipping.";

    public void EnsureIsPreserved()
    {
        if (currentStatus != OrderStatus.Paid)
        {
            throw new OrderMustBePaidBeforePreparingForShippingException();
        }
    }
}
