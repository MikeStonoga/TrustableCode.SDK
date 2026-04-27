using TrustableCode.SDK.BusinessModeling.Compensation;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record InvoiceApprovalCompensationResult(
    CompensationRequest<InvoiceApprovalId> CompensationRequest,
    InvoiceApprovalCompensationScheduledEvidence TransitionEvidence,
    CompensationScheduledEvidence CompensationEvidence);
