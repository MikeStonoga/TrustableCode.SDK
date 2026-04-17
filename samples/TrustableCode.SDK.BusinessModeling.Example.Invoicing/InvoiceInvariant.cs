namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public enum InvoiceInvariant
{
    RefundMustNotExceedCapturedAmount = 1,
    RefundRequiresCapturedInvoice = 2
}
