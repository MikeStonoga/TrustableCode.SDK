using TrustableCode.SDK.BusinessModeling.Events;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessEventOutboxBatchTests
{
    [Fact]
    public void CreateFrom_should_wrap_dequeued_events_as_outbox_messages()
    {
        var source = SampleSource.Create("prepare-order");
        var enqueuedAt = new DateTimeOffset(2026, 4, 17, 23, 45, 0, TimeSpan.Zero);

        var messages = BusinessEventOutboxBatch.CreateFrom(
            streamName: "Order-123",
            source: source,
            enqueuedAt: enqueuedAt);

        Assert.Single(messages);
        Assert.Equal("Order-123", messages[0].StreamName);
        Assert.Equal(nameof(SampleEvent), messages[0].EventName);
        Assert.Equal(enqueuedAt, messages[0].EnqueuedAt);
        Assert.IsType<SampleEvent>(messages[0].BusinessEvent);
        Assert.Equal(messages[0].BusinessEvent.EventId, messages[0].EventId);
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
