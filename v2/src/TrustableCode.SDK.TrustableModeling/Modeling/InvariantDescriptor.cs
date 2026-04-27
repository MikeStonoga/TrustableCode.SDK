namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes a business truth that must survive change.
/// </summary>
public sealed record InvariantDescriptor
{
    public InvariantDescriptor(
        string code,
        string name,
        string description,
        InvariantSeverity severity = InvariantSeverity.Critical)
    {
        Code = Require.Text(code, nameof(code));
        Name = Require.Text(name, nameof(name));
        Description = Require.Text(description, nameof(description));
        Severity = severity;
    }

    public string Code { get; }

    public string Name { get; }

    public string Description { get; }

    public InvariantSeverity Severity { get; }
}

