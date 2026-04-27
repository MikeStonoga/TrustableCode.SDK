namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes evidence the system should produce so trust can be justified after change.
/// </summary>
public sealed record EvidenceContract
{
    public EvidenceContract(string name, EvidenceKind kind, string description)
    {
        Name = Require.Text(name, nameof(name));
        Kind = kind;
        Description = Require.Text(description, nameof(description));
    }

    public string Name { get; }

    public EvidenceKind Kind { get; }

    public string Description { get; }
}

