using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class InvoiceApprovalCompensationSampleTests
{
    [Fact]
    public void ScheduleApprovalCompensation_should_record_transition_and_durable_request()
    {
        var approval = InvoiceApproval.Create(new CreateInvoiceApprovalRequirement(
            InvoiceApprovalId.New(),
            new DateTimeOffset(2026, 4, 17, 23, 0, 0, TimeSpan.Zero)));

        var result = approval.ScheduleApprovalCompensation(new ScheduleInvoiceApprovalCompensationRequirement(
            DownstreamRejectionReason: "Tax authority rejected the approval downstream.",
            RequestedAt: new DateTimeOffset(2026, 4, 17, 23, 5, 0, TimeSpan.Zero),
            CorrelationId: "corr-approval-1"));

        var businessEvents = approval.DequeueBusinessEvents();
        var businessEvidence = approval.DequeueBusinessEvidence();

        Assert.True(approval.IsApprovalCompensationPending);
        Assert.Equal("Tax authority rejected the approval downstream.", approval.DownstreamRejectionReason);
        Assert.Equal("ScheduleApprovalCompensation", result.CompensationRequest.CompensationName);
        Assert.Single(businessEvents);
        Assert.Equal(2, businessEvidence.Count);
        Assert.Contains(businessEvidence, evidence => evidence is CompensationScheduledEvidence);
    }

    [Fact]
    public void ScheduleApprovalCompensation_should_reject_non_approved_state()
    {
        var approval = InvoiceApproval.Create(new CreateInvoiceApprovalRequirement(
            InvoiceApprovalId.New(),
            DateTimeOffset.UtcNow));

        approval.ScheduleApprovalCompensation(new ScheduleInvoiceApprovalCompensationRequirement(
            DownstreamRejectionReason: "First downstream rejection",
            RequestedAt: DateTimeOffset.UtcNow,
            CorrelationId: "corr-approval-2"));

        var exception = Assert.Throws<BusinessRuleViolationException>(() =>
            approval.ScheduleApprovalCompensation(new ScheduleInvoiceApprovalCompensationRequirement(
                DownstreamRejectionReason: "Second downstream rejection",
                RequestedAt: DateTimeOffset.UtcNow,
                CorrelationId: "corr-approval-3")));

        Assert.Equal("Only approved invoices may schedule approval compensation.", exception.Message);
    }
}
