namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

/// <summary>
/// Result returned by the invoicing application service after a successful refund workflow.
/// </summary>
public sealed record InvoiceRefundResult(
    InvoiceId InvoiceId,
    InvoiceStatus CurrentStatus,
    string CompletedTransition,
    int EmittedBusinessEvents);
