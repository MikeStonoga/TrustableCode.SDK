namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes how tightly a side effect must align with the state transition that caused it.
/// </summary>
public enum SideEffectConsistency
{
    Immediate = 0,
    TransactionalOutbox = 1,
    EventuallyConsistent = 2,
    ExternalConfirmationRequired = 3
}

