using TrustableCode.SDK.BusinessModeling.Exceptions;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing.Exceptions;

public sealed class RefundRequiresCapturedInvoiceException : BusinessRuleViolationException
{
    public RefundRequiresCapturedInvoiceException()
        : base("Only captured or partially refunded invoices can be refunded.")
    {
    }
}
