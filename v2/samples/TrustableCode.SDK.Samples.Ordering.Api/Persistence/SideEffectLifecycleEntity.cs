namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class SideEffectLifecycleEntity
{
    public string IdempotencyKey { get; set; } = string.Empty;

    public string SideEffectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string EvidenceJson { get; set; } = "{}";

    public string HistoryJson { get; set; } = "[]";
}
