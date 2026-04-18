using TrustableCode.SDK.BusinessModeling.Events;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record InvoiceApprovalCompensationScheduled(
    InvoiceApprovalId InvoiceApprovalId,
    Guid CompensationId,
    string DownstreamRejectionReason,
    DateTimeOffset OccurredAt) : BusinessEvent(OccurredAt);
