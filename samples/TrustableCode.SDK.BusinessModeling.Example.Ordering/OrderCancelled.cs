using TrustableCode.SDK.BusinessModeling.Events;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record OrderCancelled(
    OrderId OrderId,
    DateTimeOffset OccurredAt) : BusinessEvent(OccurredAt);
