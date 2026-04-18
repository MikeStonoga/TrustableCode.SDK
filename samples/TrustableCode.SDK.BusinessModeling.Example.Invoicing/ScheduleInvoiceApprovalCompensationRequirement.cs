namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record ScheduleInvoiceApprovalCompensationRequirement(
    string DownstreamRejectionReason,
    DateTimeOffset RequestedAt,
    string CorrelationId);
