using TrustableCode.SDK.BusinessModeling.Example.Invoicing.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing.Invariants;

public sealed class RefundMustNotExceedCapturedAmountRule(Money capturedAmount, Money alreadyRefunded, Money requestedRefund) : IBusinessInvariantRule
{
    public string Description => "Refund must never exceed the captured amount.";

    public void EnsureIsPreserved()
    {
        var wouldExceedCapturedAmount = alreadyRefunded.Amount + requestedRefund.Amount > capturedAmount.Amount;

        if (wouldExceedCapturedAmount)
        {
            throw new RefundMustNotExceedCapturedAmountException();
        }
    }
}
