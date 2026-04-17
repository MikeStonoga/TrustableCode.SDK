namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record CreateInvoiceRequirement(
    InvoiceId InvoiceId,
    Money CapturedAmount);
