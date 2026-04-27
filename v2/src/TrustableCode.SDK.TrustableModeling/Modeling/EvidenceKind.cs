namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Classifies the business evidence expected from a trustable model.
/// </summary>
public enum EvidenceKind
{
    Transition = 0,
    InvariantViolation = 1,
    BoundaryRejection = 2,
    SideEffect = 3,
    Reconciliation = 4,
    Compensation = 5,
    Audit = 6
}

