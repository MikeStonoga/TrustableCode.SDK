namespace TrustableCode.SDK.BusinessModeling.Invariants;

/// <summary>
/// A focused, testable rule that protects an important business truth.
/// </summary>
public interface IBusinessInvariantRule
{
    /// <summary>
    /// Human-readable description of the protected business truth.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Throws when the business truth is not preserved.
    /// </summary>
    void EnsureIsPreserved();
}
