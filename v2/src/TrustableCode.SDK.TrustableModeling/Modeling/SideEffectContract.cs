namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes a side effect that must be governed when the model changes.
/// </summary>
public sealed record SideEffectContract
{
    public SideEffectContract(
        string name,
        string description,
        SideEffectConsistency consistency,
        bool requiresIdempotencyKey = false,
        bool requiresCompensation = false)
    {
        Name = Require.Text(name, nameof(name));
        Description = Require.Text(description, nameof(description));
        Consistency = consistency;
        RequiresIdempotencyKey = requiresIdempotencyKey;
        RequiresCompensation = requiresCompensation;
    }

    public string Name { get; }

    public string Description { get; }

    public SideEffectConsistency Consistency { get; }

    public bool RequiresIdempotencyKey { get; }

    public bool RequiresCompensation { get; }
}

