namespace TrustableCode.SDK.TrustableModeling.Admission;

/// <summary>
/// Fluent builder for declaring boundary admission rules without repeating plumbing.
/// </summary>
public sealed class BusinessAdmissionBuilder<TInput, TAccepted>
    where TInput : notnull
    where TAccepted : notnull
{
    private readonly string _name;
    private readonly List<AdmissionRule<TInput>> _rules = [];
    private Func<TInput, TAccepted>? _accept;

    internal BusinessAdmissionBuilder(string name)
    {
        _name = TrustableModeling.Require.Text(name, nameof(name));
    }

    public BusinessAdmissionBuilder<TInput, TAccepted> Require(
        string code,
        string description,
        Func<TInput, bool> isSatisfied,
        string rejectionReason,
        string? rejectionEvidenceName = null)
    {
        _rules.Add(new AdmissionRule<TInput>(
            code,
            description,
            isSatisfied,
            rejectionReason,
            rejectionEvidenceName));

        return this;
    }

    public BusinessAdmissionBuilder<TInput, TAccepted> RejectWhen(
        string code,
        string description,
        Func<TInput, bool> shouldReject,
        string rejectionReason,
        string? rejectionEvidenceName = null)
    {
        ArgumentNullException.ThrowIfNull(shouldReject);

        return Require(
            code,
            description,
            input => !shouldReject(input),
            rejectionReason,
            rejectionEvidenceName);
    }

    public BusinessAdmissionBuilder<TInput, TAccepted> AcceptWith(Func<TInput, TAccepted> accept)
    {
        _accept = accept ?? throw new ArgumentNullException(nameof(accept));
        return this;
    }

    public BusinessAdmission<TInput, TAccepted> Build()
    {
        if (_accept is null)
        {
            throw new InvalidOperationException(
                $"Admission '{_name}' must declare an accept function before it can be built.");
        }

        return new BusinessAdmission<TInput, TAccepted>(_name, _rules, _accept);
    }
}
