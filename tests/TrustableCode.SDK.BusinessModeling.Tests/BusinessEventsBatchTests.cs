using TrustableCode.SDK.BusinessModeling.Events;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessEventsBatchTests
{
    [Fact]
    public void CreateFrom_should_merge_events_from_multiple_sources()
    {
        var left = SampleSource.Create("left");
        var right = SampleSource.Create("right");

        var batch = BusinessEventsBatch.CreateFrom(left, right);

        Assert.Equal(2, batch.Count);
        Assert.Contains(batch, x => ((SampleEvent)x).Name == "left");
        Assert.Contains(batch, x => ((SampleEvent)x).Name == "right");
    }

    private sealed class SampleSource : BusinessEntity
    {
        public static SampleSource Create(string name)
        {
            var source = new SampleSource();
            source.RecordBusinessEvent(new SampleEvent(name, DateTimeOffset.UtcNow));
            return source;
        }
    }

    private sealed record SampleEvent(string Name, DateTimeOffset OccurredAt) : BusinessEvent(OccurredAt);
}
