namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Names one meaningful state in a business model.
/// </summary>
public sealed record StateDefinition
{
    public StateDefinition(
        string name,
        string description,
        bool isInitial = false,
        bool isTerminal = false)
    {
        Name = Require.Text(name, nameof(name));
        Description = Require.Text(description, nameof(description));
        IsInitial = isInitial;
        IsTerminal = isTerminal;
    }

    public string Name { get; }

    public string Description { get; }

    public bool IsInitial { get; }

    public bool IsTerminal { get; }
}
