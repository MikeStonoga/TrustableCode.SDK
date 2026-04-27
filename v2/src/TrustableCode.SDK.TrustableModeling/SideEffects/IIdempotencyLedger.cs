namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Tracks whether an externally meaningful side effect has already been applied.
/// </summary>
public interface IIdempotencyLedger
{
    bool HasBeenApplied(string key);

    void MarkApplied(string key);
}

