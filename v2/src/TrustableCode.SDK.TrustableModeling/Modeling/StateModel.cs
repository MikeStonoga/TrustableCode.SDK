namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes the authoritative state representation for a model.
/// </summary>
public sealed record StateModel
{
    public StateModel(string authoritativeState, IEnumerable<StateDefinition> states)
    {
        AuthoritativeState = Require.Text(authoritativeState, nameof(authoritativeState));
        States = Require.NotEmpty(states, nameof(states));
    }

    public string AuthoritativeState { get; }

    public IReadOnlyList<StateDefinition> States { get; }
}

