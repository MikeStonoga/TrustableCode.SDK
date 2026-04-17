using TrustableCode.SDK.BusinessModeling.Events;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record OrderPreparedForShipping(
    OrderId OrderId,
    DateTimeOffset OccurredAt) : BusinessEvent(OccurredAt);
