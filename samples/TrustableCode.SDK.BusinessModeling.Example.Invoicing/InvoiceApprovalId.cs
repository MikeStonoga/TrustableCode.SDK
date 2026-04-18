using TrustableCode.SDK.BusinessModeling.Identity;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed class InvoiceApprovalId(Guid value) : TypedId<Guid>(value)
{
    public static InvoiceApprovalId New()
        => new(Guid.NewGuid());
}
