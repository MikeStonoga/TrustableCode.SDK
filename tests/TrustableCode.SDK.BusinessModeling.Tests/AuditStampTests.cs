using TrustableCode.SDK.BusinessModeling.Auditing;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class AuditStampTests
{
    [Fact]
    public void CreateForModification_should_capture_actor_time_and_action()
    {
        var occurredAt = new DateTimeOffset(2026, 4, 17, 23, 0, 0, TimeSpan.Zero);
        var stamp = AuditStamp.CreateForModification("mike", occurredAt);

        Assert.Equal("mike", stamp.Actor);
        Assert.Equal(occurredAt, stamp.OccurredAt);
        Assert.Equal(AuditAction.Modification, stamp.Action);
    }
}
