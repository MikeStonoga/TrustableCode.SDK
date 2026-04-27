namespace TrustableCode.SDK.TrustableModeling.SideEffects;

/// <summary>
/// Stores the durable lifecycle record of planned external effects.
/// </summary>
public interface ISideEffectLifecycleStore
{
    SideEffectLifecycleRecord Save(SideEffectLifecycleRecord record);

    SideEffectLifecycleRecord? Find(string idempotencyKey);

    IReadOnlyCollection<SideEffectLifecycleRecord> All();
}
