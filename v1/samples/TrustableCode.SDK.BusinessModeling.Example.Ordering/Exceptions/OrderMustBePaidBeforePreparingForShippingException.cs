using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;

public sealed class OrderMustBePaidBeforePreparingForShippingException : BusinessRuleViolationException
{
    public OrderMustBePaidBeforePreparingForShippingException()
        : base("Order must be paid before it can be prepared for shipping.")
    {
    }
}
