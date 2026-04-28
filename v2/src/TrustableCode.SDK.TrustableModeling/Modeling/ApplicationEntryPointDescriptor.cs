namespace TrustableCode.SDK.TrustableModeling.Modeling;

public sealed record ApplicationEntryPointDescriptor(
    string name,
    string description,
    string whenToUse,
    IReadOnlyList<string>? reads = null,
    IReadOnlyList<string>? writes = null,
    IReadOnlyList<string>? emits = null)
{
    public string Name { get; } = Require.Text(name, nameof(name));

    public string Description { get; } = Require.Text(description, nameof(description));

    public string WhenToUse { get; } = Require.Text(whenToUse, nameof(whenToUse));

    public IReadOnlyList<string> Reads { get; } = Require.TextList(reads);

    public IReadOnlyList<string> Writes { get; } = Require.TextList(writes);

    public IReadOnlyList<string> Emits { get; } = Require.TextList(emits);
}
