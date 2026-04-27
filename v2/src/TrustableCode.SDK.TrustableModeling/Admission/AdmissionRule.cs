namespace TrustableCode.SDK.TrustableModeling.Admission;

/// <summary>
/// A named rule that decides whether external input may be admitted as business meaning.
/// </summary>
public sealed class AdmissionRule<TInput>
    where TInput : notnull
{
    private readonly Func<TInput, bool> _isSatisfied;

    public AdmissionRule(
        string code,
        string description,
        Func<TInput, bool> isSatisfied,
        string rejectionReason,
        string? rejectionEvidenceName = null)
    {
        Code = Require.Text(code, nameof(code));
        Description = Require.Text(description, nameof(description));
        _isSatisfied = isSatisfied ?? throw new ArgumentNullException(nameof(isSatisfied));
        RejectionReason = Require.Text(rejectionReason, nameof(rejectionReason));
        RejectionEvidenceName = string.IsNullOrWhiteSpace(rejectionEvidenceName)
            ? $"{Code}Rejected"
            : rejectionEvidenceName;
    }

    public string Code { get; }

    public string Description { get; }

    public string RejectionReason { get; }

    public string RejectionEvidenceName { get; }

    public bool IsSatisfiedBy(TInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return _isSatisfied(input);
    }
}
