namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record CreateInvoiceApprovalRequirement(
    InvoiceApprovalId InvoiceApprovalId,
    DateTimeOffset ApprovedAt);
