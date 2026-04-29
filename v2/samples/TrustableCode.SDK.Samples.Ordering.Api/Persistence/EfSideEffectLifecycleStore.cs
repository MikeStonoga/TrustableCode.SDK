using System.Text.Json;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Modeling;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class EfSideEffectLifecycleStore(OrderingDbContext db) : ISideEffectLifecycleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public SideEffectLifecycleRecord Save(SideEffectLifecycleRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var entity = db.SideEffectLifecycles.Find(record.IdempotencyKey);
        if (entity is null)
        {
            db.SideEffectLifecycles.Add(ToEntity(record));
        }
        else
        {
            entity.SideEffectName = record.SideEffectName;
            entity.Status = record.Status.ToString();
            entity.EvidenceJson = SerializeEvidence(record.Evidence);
            entity.HistoryJson = SerializeEvidence(record.History);
        }

        return record;
    }

    public SideEffectLifecycleRecord? Find(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        var entity = db.SideEffectLifecycles.Find(idempotencyKey);

        return entity is null ? null : ToRecord(entity);
    }

    public IReadOnlyCollection<SideEffectLifecycleRecord> All()
        => db.SideEffectLifecycles
            .OrderBy(lifecycle => lifecycle.IdempotencyKey)
            .Select(ToRecord)
            .ToArray();

    private static SideEffectLifecycleEntity ToEntity(SideEffectLifecycleRecord record)
        => new()
        {
            IdempotencyKey = record.IdempotencyKey,
            SideEffectName = record.SideEffectName,
            Status = record.Status.ToString(),
            EvidenceJson = SerializeEvidence(record.Evidence),
            HistoryJson = SerializeEvidence(record.History)
        };

    private static SideEffectLifecycleRecord ToRecord(SideEffectLifecycleEntity entity)
    {
        var status = Enum.Parse<SideEffectLifecycleStatus>(entity.Status);
        var evidence = DeserializeEvidence(entity.EvidenceJson, entity.IdempotencyKey);
        var history = DeserializeHistory(entity.HistoryJson, entity.IdempotencyKey);

        return new SideEffectLifecycleRecord(
            entity.SideEffectName,
            entity.IdempotencyKey,
            status,
            evidence,
            history);
    }

    private static string SerializeEvidence(BusinessEvidence evidence)
        => JsonSerializer.Serialize(PersistedEvidence.From(evidence), JsonOptions);

    private static string SerializeEvidence(IReadOnlyList<BusinessEvidence> evidence)
        => JsonSerializer.Serialize(evidence.Select(PersistedEvidence.From).ToArray(), JsonOptions);

    private static BusinessEvidence DeserializeEvidence(string json, string idempotencyKey)
    {
        var evidence = JsonSerializer.Deserialize<PersistedEvidence>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Side effect lifecycle '{idempotencyKey}' has invalid evidence JSON.");

        return evidence.ToBusinessEvidence();
    }

    private static IReadOnlyList<BusinessEvidence> DeserializeHistory(string json, string idempotencyKey)
    {
        var history = JsonSerializer.Deserialize<PersistedEvidence[]>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Side effect lifecycle '{idempotencyKey}' has invalid history JSON.");

        return history.Select(evidence => evidence.ToBusinessEvidence()).ToArray();
    }

    private sealed record PersistedEvidence(
        string Name,
        EvidenceKind Kind,
        string Message,
        string? CorrelationId,
        DateTimeOffset ObservedAt,
        IReadOnlyDictionary<string, string> Metadata)
    {
        public static PersistedEvidence From(BusinessEvidence evidence)
            => new(
                evidence.Name,
                evidence.Kind,
                evidence.Message,
                evidence.CorrelationId,
                evidence.ObservedAt,
                evidence.Metadata);

        public BusinessEvidence ToBusinessEvidence()
            => new(Name, Kind, Message, CorrelationId, ObservedAt, Metadata);
    }
}
