using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;

public sealed class StockMustBeReservedBeforePreparingOrderForShippingException : BusinessRuleViolationException
{
    public StockMustBeReservedBeforePreparingOrderForShippingException()
        : base("Stock must be reserved before the order can be prepared for shipping.")
    {
    }
}
