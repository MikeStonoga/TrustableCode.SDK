using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Invariants;

/// <summary>
/// Result of evaluating one business invariant against a context.
/// </summary>
public sealed record InvariantEvaluation
{
    public InvariantEvaluation(
        string code,
        string name,
        InvariantSeverity severity,
        bool isPreserved,
        string message)
    {
        Code = Require.Text(code, nameof(code));
        Name = Require.Text(name, nameof(name));
        Severity = severity;
        IsPreserved = isPreserved;
        Message = Require.Text(message, nameof(message));
    }

    public string Code { get; }

    public string Name { get; }

    public InvariantSeverity Severity { get; }

    public bool IsPreserved { get; }

    public string Message { get; }
}

