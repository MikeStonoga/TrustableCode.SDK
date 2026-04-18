using TrustableCode.SDK.BusinessModeling.Compensation;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed record InvoiceApprovalCompensationDecision(
    CompensationDecision Compensation,
    string DownstreamRejectionReason)
{
    public Guid CompensationId => Compensation.CompensationId;

    public static InvoiceApprovalCompensationDecision Create(ScheduleInvoiceApprovalCompensationRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(requirement);

        var compensation = CompensationDecision.Create(
            reason: requirement.DownstreamRejectionReason,
            decidedAt: requirement.RequestedAt);

        return new InvoiceApprovalCompensationDecision(compensation, compensation.Reason);
    }
}
