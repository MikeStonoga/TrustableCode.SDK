using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Evidence;

/// <summary>
/// Structured business evidence emitted when a model accepts, rejects, changes, or observes important meaning.
/// </summary>
public sealed record BusinessEvidence
{
    public BusinessEvidence(
        string name,
        EvidenceKind kind,
        string message,
        string? correlationId = null,
        DateTimeOffset? observedAt = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Name = Require.Text(name, nameof(name));
        Kind = kind;
        Message = Require.Text(message, nameof(message));
        CorrelationId = correlationId;
        ObservedAt = observedAt ?? DateTimeOffset.UtcNow;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    public string Name { get; }

    public EvidenceKind Kind { get; }

    public string Message { get; }

    public string? CorrelationId { get; }

    public DateTimeOffset ObservedAt { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}

