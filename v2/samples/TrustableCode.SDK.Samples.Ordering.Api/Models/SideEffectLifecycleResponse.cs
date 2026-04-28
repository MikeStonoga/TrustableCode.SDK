using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.Samples.Ordering.Api.Models;

public sealed record SideEffectLifecycleResponse(
    string SideEffectName,
    string IdempotencyKey,
    string Status)
{
    public static SideEffectLifecycleResponse From(SideEffectLifecycleRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new SideEffectLifecycleResponse(
            record.SideEffectName,
            record.IdempotencyKey,
            record.Status.ToString());
    }
}
