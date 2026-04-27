namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

/// <summary>
/// Result returned by the invoicing application service after a successful approval compensation scheduling workflow.
/// </summary>
public sealed record InvoiceApprovalCompensationApplicationResult(
    InvoiceApprovalId InvoiceApprovalId,
    InvoiceApprovalStatus CurrentStatus,
    string CompletedTransition,
    int EmittedBusinessEvents);
