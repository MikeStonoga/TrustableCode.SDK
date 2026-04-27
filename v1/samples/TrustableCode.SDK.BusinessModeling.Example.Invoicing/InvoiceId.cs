using TrustableCode.SDK.BusinessModeling.Identity;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed class InvoiceId(Guid value) : TypedId<Guid>(value)
{
    public static InvoiceId New() => new(Guid.NewGuid());
}
