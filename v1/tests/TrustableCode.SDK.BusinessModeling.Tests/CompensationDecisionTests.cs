using TrustableCode.SDK.BusinessModeling.Compensation;
using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class CompensationDecisionTests
{
    [Fact]
    public void Create_should_require_reason()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() =>
            CompensationDecision.Create("   ", DateTimeOffset.UtcNow));

        Assert.Equal("A compensation decision requires a reason.", exception.Message);
    }

    [Fact]
    public void CreateFor_should_build_durable_request_from_explicit_decision()
    {
        var approvalId = InvoiceApprovalId.New();
        var decision = CompensationDecision.Create("Downstream invoice rejection", DateTimeOffset.UtcNow);

        var request = CompensationRequest<InvoiceApprovalId>.CreateFor(
            subjectId: approvalId,
            compensationName: "ScheduleApprovalCompensation",
            decision: decision);

        Assert.Equal(approvalId, request.SubjectId);
        Assert.Equal("ScheduleApprovalCompensation", request.CompensationName);
        Assert.Equal("Downstream invoice rejection", request.Reason);
    }
}
