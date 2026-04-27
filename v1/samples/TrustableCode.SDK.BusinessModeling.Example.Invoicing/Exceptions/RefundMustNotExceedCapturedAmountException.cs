using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing.Exceptions;

public sealed class RefundMustNotExceedCapturedAmountException : BusinessRuleViolationException
{
    public RefundMustNotExceedCapturedAmountException()
        : base("Refund must not exceed the captured amount.")
    {
    }
}
