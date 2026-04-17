using TrustableCode.SDK.BusinessModeling.Identity;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed class OrderId(Guid value) : TypedId<Guid>(value)
{
    public static OrderId New() => new(Guid.NewGuid());
}
