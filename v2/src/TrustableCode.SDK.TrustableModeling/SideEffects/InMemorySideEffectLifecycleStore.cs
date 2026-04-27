using System.Collections.Concurrent;

namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// In-memory lifecycle store for tests, samples, and local diagnostics.
/// </summary>
public sealed class InMemorySideEffectLifecycleStore : ISideEffectLifecycleStore
{
    private readonly ConcurrentDictionary<string, SideEffectLifecycleRecord> _records = new();

    public SideEffectLifecycleRecord Save(SideEffectLifecycleRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        _records[record.IdempotencyKey] = record;
        return record;
    }

    public SideEffectLifecycleRecord? Find(string idempotencyKey)
    {
        idempotencyKey = Require.Text(idempotencyKey, nameof(idempotencyKey));

        return _records.TryGetValue(idempotencyKey, out var record)
            ? record
            : null;
    }

    public IReadOnlyCollection<SideEffectLifecycleRecord> All()
        => _records.Values.ToArray();
}
