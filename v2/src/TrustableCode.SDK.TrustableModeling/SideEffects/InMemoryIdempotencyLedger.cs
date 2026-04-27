namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Minimal in-memory idempotency ledger for tests and samples.
/// </summary>
public sealed class InMemoryIdempotencyLedger : IIdempotencyLedger
{
    private readonly HashSet<string> _appliedKeys = [];

    public bool HasBeenApplied(string key)
        => _appliedKeys.Contains(Require.Text(key, nameof(key)));

    public void MarkApplied(string key)
        => _appliedKeys.Add(Require.Text(key, nameof(key)));
}

