namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class BusinessEvidenceEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }

    public DateTimeOffset ObservedAt { get; set; }

    public string MetadataJson { get; set; } = "{}";
}
