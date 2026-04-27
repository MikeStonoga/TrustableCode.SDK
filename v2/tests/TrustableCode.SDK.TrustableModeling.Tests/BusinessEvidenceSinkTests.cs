using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.TrustableModeling.Evidence;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class BusinessEvidenceSinkTests
{
    [Fact]
    public void In_memory_sink_should_capture_business_evidence()
    {
        var sink = new InMemoryBusinessEvidenceSink();
        var evidence = new BusinessEvidence(
            name: "OrderPreparationRejectedEvidence",
            kind: Modeling.EvidenceKind.InvariantViolation,
            message: "Order preparation was rejected.",
            correlationId: "corr-evidence-1");

        sink.Record(evidence);

        Assert.Single(sink.Evidence);
        Assert.Equal("OrderPreparationRejectedEvidence", sink.Evidence[0].Name);
        Assert.Equal("corr-evidence-1", sink.Evidence[0].CorrelationId);
    }

    [Fact]
    public void Composite_sink_should_dispatch_evidence_to_all_sinks()
    {
        var first = new InMemoryBusinessEvidenceSink();
        var second = new InMemoryBusinessEvidenceSink();
        var composite = new CompositeBusinessEvidenceSink([first, second]);

        composite.Record(new BusinessEvidence(
            name: "BoundaryRejected",
            kind: Modeling.EvidenceKind.BoundaryRejection,
            message: "Boundary rejected invalid input."));

        Assert.Single(first.Evidence);
        Assert.Single(second.Evidence);
    }

    [Fact]
    public void Ordering_evidence_publisher_should_publish_order_business_evidence()
    {
        var order = new Order(OrderStatus.Paid);
        order.PrepareForShipping(new PrepareOrderForShippingRequirement(
            PaymentCaptured: false,
            StockReserved: false,
            CorrelationId: "corr-order-evidence"));

        var sink = new InMemoryBusinessEvidenceSink();
        var publisher = new OrderingEvidencePublisher(sink);

        publisher.Publish(order);

        Assert.Equal(2, sink.Evidence.Count);
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "PaymentCapturedBeforeShipmentPreparationViolation");
        Assert.Contains(sink.Evidence, evidence => evidence.Name == "StockReservedBeforeShipmentPreparationViolation");
    }

    [Fact]
    public void Activity_source_sink_should_emit_business_evidence_as_activity_tags()
    {
        using var activitySource = new ActivitySource("TrustableCode.Tests");
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "TrustableCode.Tests",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };

        var stoppedActivities = new List<Activity>();
        listener.ActivityStopped = activity => stoppedActivities.Add(activity);
        ActivitySource.AddActivityListener(listener);

        var sink = new ActivitySourceBusinessEvidenceSink(activitySource);
        sink.Record(new BusinessEvidence(
            name: "OrderPreparationRejectedEvidence",
            kind: Modeling.EvidenceKind.InvariantViolation,
            message: "Order preparation was rejected.",
            correlationId: "corr-trace-1",
            metadata: new Dictionary<string, string>
            {
                ["invariant.code"] = "PaymentCapturedBeforeShipmentPreparation"
            }));

        Assert.Single(stoppedActivities);

        var tags = stoppedActivities[0].Tags.ToDictionary(tag => tag.Key, tag => tag.Value);
        Assert.Equal("business.evidence.InvariantViolation", stoppedActivities[0].OperationName);
        Assert.Equal("OrderPreparationRejectedEvidence", tags["business.evidence.name"]);
        Assert.Equal("InvariantViolation", tags["business.evidence.kind"]);
        Assert.Equal("corr-trace-1", tags["business.evidence.correlation_id"]);
        Assert.Equal("PaymentCapturedBeforeShipmentPreparation", tags["business.metadata.invariant.code"]);
    }

    [Fact]
    public void Logger_sink_should_emit_business_evidence_as_structured_log_state()
    {
        var logger = new CapturingLogger();
        var sink = new LoggerBusinessEvidenceSink(logger);

        sink.Record(new BusinessEvidence(
            name: "OrderPreparationRejectedEvidence",
            kind: Modeling.EvidenceKind.InvariantViolation,
            message: "Order preparation was rejected.",
            correlationId: "corr-log-1",
            metadata: new Dictionary<string, string>
            {
                ["invariant.code"] = "PaymentCapturedBeforeShipmentPreparation"
            }));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("OrderPreparationRejectedEvidence", entry.EventId.Name);
        Assert.Equal("Order preparation was rejected.", entry.Message);
        Assert.Equal("InvariantViolation", entry.State["business.evidence.kind"]);
        Assert.Equal("corr-log-1", entry.State["business.evidence.correlation_id"]);
        Assert.Equal("PaymentCapturedBeforeShipmentPreparation", entry.State["business.metadata.invariant.code"]);
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var structuredState = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, object?>();

            Entries.Add(new CapturedLogEntry(
                logLevel,
                eventId,
                formatter(state, exception),
                structuredState));
        }
    }

    private sealed record CapturedLogEntry(
        LogLevel Level,
        EventId EventId,
        string Message,
        IReadOnlyDictionary<string, object?> State);
}
