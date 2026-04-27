namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Observable lifecycle of an external side effect.
/// </summary>
public enum SideEffectLifecycleStatus
{
    Planned = 0,
    Persisted = 1,
    Published = 2,
    Confirmed = 3,
    CompensationRequired = 4,
    Compensated = 5
}
