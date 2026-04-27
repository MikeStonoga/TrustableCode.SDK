namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes a legitimate movement between meaningful states.
/// </summary>
public sealed record BusinessTransitionDescriptor
{
    public BusinessTransitionDescriptor(
        string name,
        string fromState,
        string toState,
        string description,
        IEnumerable<string>? preconditions = null,
        IEnumerable<string>? producedEvents = null,
        IEnumerable<string>? producedEvidence = null)
    {
        Name = Require.Text(name, nameof(name));
        FromState = Require.Text(fromState, nameof(fromState));
        ToState = Require.Text(toState, nameof(toState));
        Description = Require.Text(description, nameof(description));
        Preconditions = Require.TextList(preconditions);
        ProducedEvents = Require.TextList(producedEvents);
        ProducedEvidence = Require.TextList(producedEvidence);
    }

    public string Name { get; }

    public string FromState { get; }

    public string ToState { get; }

    public string Description { get; }

    public IReadOnlyList<string> Preconditions { get; }

    public IReadOnlyList<string> ProducedEvents { get; }

    public IReadOnlyList<string> ProducedEvidence { get; }
}

