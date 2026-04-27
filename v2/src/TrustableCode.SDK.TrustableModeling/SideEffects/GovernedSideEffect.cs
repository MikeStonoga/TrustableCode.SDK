using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Executes an external effect only when idempotency and evidence requirements are satisfied.
/// </summary>
public sealed class GovernedSideEffect<TContext>
    where TContext : notnull
{
    private readonly Func<TContext, string?> _idempotencyKey;
    private readonly Action<TContext> _execute;
    private readonly IIdempotencyLedger _ledger;

    public GovernedSideEffect(
        string name,
        Func<TContext, string?> idempotencyKey,
        Action<TContext> execute,
        IIdempotencyLedger ledger)
    {
        Name = Require.Text(name, nameof(name));
        _idempotencyKey = idempotencyKey ?? throw new ArgumentNullException(nameof(idempotencyKey));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
    }

    public string Name { get; }

    public SideEffectExecutionResult Execute(TContext context, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var key = _idempotencyKey(context);
        if (string.IsNullOrWhiteSpace(key))
        {
            var message = $"Side effect '{Name}' requires an idempotency key.";
            return new SideEffectExecutionResult(
                Name,
                SideEffectExecutionStatus.Rejected,
                key,
                CreateEvidence(SideEffectExecutionStatus.Rejected, message, key, correlationId),
                message);
        }

        if (_ledger.HasBeenApplied(key))
        {
            var message = $"Side effect '{Name}' was already applied for idempotency key '{key}'.";
            return new SideEffectExecutionResult(
                Name,
                SideEffectExecutionStatus.AlreadyApplied,
                key,
                CreateEvidence(SideEffectExecutionStatus.AlreadyApplied, message, key, correlationId));
        }

        _execute(context);
        _ledger.MarkApplied(key);

        return new SideEffectExecutionResult(
            Name,
            SideEffectExecutionStatus.Executed,
            key,
            CreateEvidence(SideEffectExecutionStatus.Executed, $"Side effect '{Name}' executed.", key, correlationId));
    }

    private BusinessEvidence CreateEvidence(
        SideEffectExecutionStatus status,
        string message,
        string? idempotencyKey,
        string? correlationId)
        => new(
            name: $"{Name}Evidence",
            kind: EvidenceKind.SideEffect,
            message: message,
            correlationId: correlationId,
            metadata: new Dictionary<string, string>
            {
                ["side_effect.name"] = Name,
                ["side_effect.status"] = status.ToString(),
                ["side_effect.idempotency_key"] = idempotencyKey ?? string.Empty
            });
}

