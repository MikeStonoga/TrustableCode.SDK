using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Plans and advances the durable lifecycle of an external side effect.
/// </summary>
public sealed class GovernedSideEffectLifecycle<TContext>
    where TContext : notnull
{
    private readonly Func<TContext, string?> _idempotencyKey;
    private readonly ISideEffectLifecycleStore _store;

    public GovernedSideEffectLifecycle(
        string name,
        Func<TContext, string?> idempotencyKey,
        ISideEffectLifecycleStore store)
    {
        Name = Require.Text(name, nameof(name));
        _idempotencyKey = idempotencyKey ?? throw new ArgumentNullException(nameof(idempotencyKey));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public string Name { get; }

    public SideEffectLifecycleRecord Plan(TContext context, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var key = Require.Text(_idempotencyKey(context), "idempotencyKey");
        var existing = _store.Find(key);
        if (existing is not null)
        {
            return existing;
        }

        var evidence = CreateEvidence(
            SideEffectLifecycleStatus.Planned,
            $"Side effect '{Name}' planned.",
            key,
            correlationId);

        return _store.Save(new SideEffectLifecycleRecord(Name, key, SideEffectLifecycleStatus.Planned, evidence));
    }

    public SideEffectLifecycleRecord Persist(string idempotencyKey, string? correlationId = null)
        => Advance(idempotencyKey, SideEffectLifecycleStatus.Persisted, "persisted", correlationId);

    public SideEffectLifecycleRecord Publish(string idempotencyKey, string? correlationId = null)
        => Advance(idempotencyKey, SideEffectLifecycleStatus.Published, "published", correlationId);

    public SideEffectLifecycleRecord Confirm(string idempotencyKey, string? correlationId = null)
        => Advance(idempotencyKey, SideEffectLifecycleStatus.Confirmed, "confirmed", correlationId);

    public SideEffectLifecycleRecord RequireCompensation(
        string idempotencyKey,
        string reason,
        string? correlationId = null)
    {
        reason = Require.Text(reason, nameof(reason));

        return Advance(
            idempotencyKey,
            SideEffectLifecycleStatus.CompensationRequired,
            "requires compensation",
            correlationId,
            new Dictionary<string, string> { ["side_effect.compensation_reason"] = reason });
    }

    public SideEffectLifecycleRecord Compensate(string idempotencyKey, string? correlationId = null)
        => Advance(idempotencyKey, SideEffectLifecycleStatus.Compensated, "compensated", correlationId);

    private SideEffectLifecycleRecord Advance(
        string idempotencyKey,
        SideEffectLifecycleStatus status,
        string verb,
        string? correlationId,
        IReadOnlyDictionary<string, string>? extraMetadata = null)
    {
        idempotencyKey = Require.Text(idempotencyKey, nameof(idempotencyKey));

        var current = _store.Find(idempotencyKey)
            ?? throw new InvalidOperationException(
                $"Side effect '{Name}' must be planned before it can be {verb}.");

        if (!string.Equals(current.SideEffectName, Name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Idempotency key '{idempotencyKey}' belongs to side effect '{current.SideEffectName}', not '{Name}'.");
        }

        var evidence = CreateEvidence(
            status,
            $"Side effect '{Name}' {verb}.",
            idempotencyKey,
            correlationId,
            extraMetadata);

        return _store.Save(current.AdvanceTo(status, evidence));
    }

    private BusinessEvidence CreateEvidence(
        SideEffectLifecycleStatus status,
        string message,
        string idempotencyKey,
        string? correlationId,
        IReadOnlyDictionary<string, string>? extraMetadata = null)
    {
        var metadata = new Dictionary<string, string>
        {
            ["side_effect.name"] = Name,
            ["side_effect.lifecycle_status"] = status.ToString(),
            ["side_effect.idempotency_key"] = idempotencyKey
        };

        if (extraMetadata is not null)
        {
            foreach (var item in extraMetadata)
            {
                metadata[item.Key] = item.Value;
            }
        }

        return new BusinessEvidence(
            name: $"{Name}{status}Evidence",
            kind: EvidenceKind.SideEffect,
            message: message,
            correlationId: correlationId,
            metadata: metadata);
    }
}
