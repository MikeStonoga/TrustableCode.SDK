using TrustableCode.SDK.BusinessModeling.Events;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record InvoiceRefunded(
    InvoiceId InvoiceId,
    decimal RefundAmount,
    string Reason,
    DateTimeOffset OccurredAt) : BusinessEvent(OccurredAt);
