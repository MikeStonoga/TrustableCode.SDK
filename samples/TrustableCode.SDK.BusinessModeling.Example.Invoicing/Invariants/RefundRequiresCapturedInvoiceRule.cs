using TrustableCode.SDK.BusinessModeling.Example.Invoicing.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing.Invariants;

public sealed class RefundRequiresCapturedInvoiceRule(InvoiceStatus currentStatus) : IBusinessInvariantRule
{
    public string Description => "Refund requires an invoice that is already captured or partially refunded.";

    public void EnsureIsPreserved()
    {
        var canBeRefunded = currentStatus is InvoiceStatus.Captured or InvoiceStatus.PartiallyRefunded;

        if (!canBeRefunded)
        {
            throw new RefundRequiresCapturedInvoiceException();
        }
    }
}
