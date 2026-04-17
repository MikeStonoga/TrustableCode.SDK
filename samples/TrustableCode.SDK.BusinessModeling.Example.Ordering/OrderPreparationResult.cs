namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

/// <summary>
/// Result returned by the ordering application service after a successful preparation workflow.
/// </summary>
public sealed record OrderPreparationResult(
    OrderId OrderId,
    OrderStatus CurrentStatus,
    string CompletedTransition,
    int EmittedBusinessEvents);
