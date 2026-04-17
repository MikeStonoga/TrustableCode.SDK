namespace TrustableCode.SDK.BusinessModeling.Auditing;

/// <summary>
/// Semantic classification for audited business actions.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// An audited creation action.
    /// </summary>
    Creation = 1,

    /// <summary>
    /// An audited modification action.
    /// </summary>
    Modification = 2,

    /// <summary>
    /// An audited deletion action.
    /// </summary>
    Deletion = 3
}
