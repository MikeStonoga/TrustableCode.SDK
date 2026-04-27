namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes what external meaning a boundary is allowed to admit into the model.
/// </summary>
public sealed record BoundaryContract
{
    public BoundaryContract(
        string name,
        string description,
        IEnumerable<string>? admissionRules = null,
        IEnumerable<string>? rejectionEvidence = null)
    {
        Name = Require.Text(name, nameof(name));
        Description = Require.Text(description, nameof(description));
        AdmissionRules = Require.TextList(admissionRules);
        RejectionEvidence = Require.TextList(rejectionEvidence);
    }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyList<string> AdmissionRules { get; }

    public IReadOnlyList<string> RejectionEvidence { get; }
}

