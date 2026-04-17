using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;

public sealed class PaymentMustBeCapturedBeforePreparingOrderForShippingException : BusinessRuleViolationException
{
    public PaymentMustBeCapturedBeforePreparingOrderForShippingException()
        : base("Payment must be captured before the order can be prepared for shipping.")
    {
    }
}
