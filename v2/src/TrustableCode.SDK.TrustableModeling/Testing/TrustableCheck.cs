namespace TrustableCode.SDK.TrustableModeling.Testing;

/// <summary>
/// Framework-neutral result for trustable model test helpers.
/// </summary>
public sealed record TrustableCheck
{
    public TrustableCheck(IEnumerable<string> failures)
    {
        Failures = failures?.Where(failure => !string.IsNullOrWhiteSpace(failure)).ToArray()
            ?? throw new ArgumentNullException(nameof(failures));
    }

    public IReadOnlyList<string> Failures { get; }

    public bool Passed => Failures.Count == 0;

    public void ThrowIfFailed()
    {
        if (!Passed)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, Failures));
        }
    }

    public static TrustableCheck Pass()
        => new([]);

    public static TrustableCheck Fail(params string[] failures)
        => new(failures);
}
