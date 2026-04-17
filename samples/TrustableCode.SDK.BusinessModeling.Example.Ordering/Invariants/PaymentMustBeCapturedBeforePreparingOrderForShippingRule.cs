using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Invariants;

public sealed class PaymentMustBeCapturedBeforePreparingOrderForShippingRule(bool paymentCaptured) : IBusinessInvariantRule
{
    public string Description => "Payment capture is required before preparing an order for shipping.";

    public void EnsureIsPreserved()
    {
        if (!paymentCaptured)
        {
            throw new PaymentMustBeCapturedBeforePreparingOrderForShippingException();
        }
    }
}
