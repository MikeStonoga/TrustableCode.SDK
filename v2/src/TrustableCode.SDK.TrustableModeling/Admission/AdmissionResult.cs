namespace TrustableCode.SDK.TrustableModeling.Admission;

/// <summary>
/// Result of attempting to convert external input into admitted business meaning.
/// </summary>
public sealed record AdmissionResult<TAccepted>
    where TAccepted : notnull
{
    private AdmissionResult(TAccepted? value, IReadOnlyList<string> rejectionReasons)
    {
        Value = value;
        RejectionReasons = rejectionReasons;
    }

    public TAccepted? Value { get; }

    public IReadOnlyList<string> RejectionReasons { get; }

    public bool WasAccepted => RejectionReasons.Count == 0;

    public static AdmissionResult<TAccepted> Accepted(TAccepted value)
        => new(value ?? throw new ArgumentNullException(nameof(value)), []);

    public static AdmissionResult<TAccepted> Rejected(IEnumerable<string> rejectionReasons)
        => new(default, Require.NotEmpty(rejectionReasons, nameof(rejectionReasons)));
}

