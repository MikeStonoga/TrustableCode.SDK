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

        var rejectionReasons = _rules
            .Where(rule => !rule.IsSatisfiedBy(input))
            .Select(rule => rule.RejectionReason)
            .ToArray();

        return rejectionReasons.Length > 0
            ? AdmissionResult<TAccepted>.Rejected(rejectionReasons)
            : AdmissionResult<TAccepted>.Accepted(_accept(input));
    }
}

