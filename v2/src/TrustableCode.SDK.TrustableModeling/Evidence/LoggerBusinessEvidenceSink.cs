using Microsoft.Extensions.Logging;
using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Emits business evidence through <see cref="ILogger"/> with stable structured fields.
/// </summary>
public sealed class LoggerBusinessEvidenceSink : IBusinessEvidenceSink
{
    private readonly ILogger _logger;

    public LoggerBusinessEvidenceSink(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Record(BusinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        _logger.Log(
            GetLogLevel(evidence.Kind),
            eventId: new EventId(0, evidence.Name),
            state: CreateState(evidence),
            exception: null,
            formatter: static (state, _) => state.Message);
    }

    private static LogLevel GetLogLevel(EvidenceKind kind)
        => kind is EvidenceKind.InvariantViolation or EvidenceKind.BoundaryRejection
            ? LogLevel.Warning
            : LogLevel.Information;

    private static BusinessEvidenceLogState CreateState(BusinessEvidence evidence)
    {
        var values = new List<KeyValuePair<string, object?>>
        {
            new("business.evidence.name", evidence.Name),
            new("business.evidence.kind", evidence.Kind.ToString()),
            new("business.evidence.message", evidence.Message),
            new("business.evidence.correlation_id", evidence.CorrelationId),
            new("business.evidence.observed_at", evidence.ObservedAt),
            new("{OriginalFormat}", "Business evidence {BusinessEvidenceName}: {BusinessEvidenceMessage}")
        };

        foreach (var (key, value) in evidence.Metadata)
        {
            values.Add(new KeyValuePair<string, object?>($"business.metadata.{key}", value));
        }

        return new BusinessEvidenceLogState(evidence.Message, values);
    }

    private sealed class BusinessEvidenceLogState(
        string message,
        IReadOnlyList<KeyValuePair<string, object?>> values)
        : IReadOnlyList<KeyValuePair<string, object?>>
    {
        public string Message { get; } = message;

        public int Count => values.Count;

        public KeyValuePair<string, object?> this[int index] => values[index];

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            => values.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

