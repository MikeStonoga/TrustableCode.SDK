namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record RefundInvoiceRequirement(
    Money RefundAmount,
    string Reason,
    DateTimeOffset RequestedAt,
    string? CorrelationId = null);
