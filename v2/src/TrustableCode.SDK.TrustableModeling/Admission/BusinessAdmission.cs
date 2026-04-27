using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.TrustableModeling.Admission;

/// <summary>
/// Converts external input into admitted business meaning only after boundary rules pass.
/// </summary>
public sealed class BusinessAdmission<TInput, TAccepted>
    where TInput : notnull
    where TAccepted : notnull
{
    private readonly IReadOnlyList<AdmissionRule<TInput>> _rules;
    private readonly Func<TInput, TAccepted> _accept;

    public BusinessAdmission(
        string name,
        IEnumerable<AdmissionRule<TInput>> rules,
        Func<TInput, TAccepted> accept)
    {
        Name = Require.Text(name, nameof(name));
        _rules = rules?.ToArray() ?? throw new ArgumentNullException(nameof(rules));
        _accept = accept ?? throw new ArgumentNullException(nameof(accept));
    }

    public string Name { get; }

    public AdmissionResult<TAccepted> Admit(TInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var rejectedRules = _rules
            .Where(rule => !rule.IsSatisfiedBy(input))
            .ToArray();

        var rejectionReasons = rejectedRules
            .Select(rule => rule.RejectionReason)
            .ToArray();

        var rejectionEvidence = rejectedRules
            .Select(rule => new BusinessEvidence(
                name: rule.RejectionEvidenceName,
                kind: EvidenceKind.BoundaryRejection,
                message: rule.RejectionReason,
                metadata: new Dictionary<string, string>
                {
                    ["admission.name"] = Name,
                    ["admission.rule.code"] = rule.Code,
                    ["admission.rule.description"] = rule.Description
                }))
            .ToArray();

        return rejectionReasons.Length > 0
            ? AdmissionResult<TAccepted>.Rejected(rejectionReasons, rejectionEvidence)
            : AdmissionResult<TAccepted>.Accepted(_accept(input));
    }
}
