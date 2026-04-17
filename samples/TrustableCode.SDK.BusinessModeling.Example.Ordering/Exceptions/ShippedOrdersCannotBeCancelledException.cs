using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;

public sealed class ShippedOrdersCannotBeCancelledException : BusinessRuleViolationException
{
    public ShippedOrdersCannotBeCancelledException()
        : base("Shipped orders cannot be cancelled.")
    {
    }
}
