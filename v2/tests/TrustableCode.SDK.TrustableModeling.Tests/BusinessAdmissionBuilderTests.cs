using TrustableCode.SDK.TrustableModeling.Admission;

namespace TrustableCode.SDK.TrustableModeling.Tests;

public sealed class BusinessAdmissionBuilderTests
{
    [Fact]
    public void Build_should_create_admission_from_declared_rules()
    {
        var admission = BusinessAdmission<ExternalCommand, AcceptedCommand>
            .Create("ExampleAdmission")
            .Require(
                code: "CorrelationIdRequired",
                description: "A correlation id is required for traceability.",
                isSatisfied: command => !string.IsNullOrWhiteSpace(command.CorrelationId),
                rejectionReason: "Correlation id is required.",
                rejectionEvidenceName: "ExampleRejectedEvidence")
            .AcceptWith(command => new AcceptedCommand(command.CorrelationId))
            .Build();

        var result = admission.Admit(new ExternalCommand(""));

        Assert.False(result.WasAccepted);
        Assert.Equal("Correlation id is required.", result.RejectionReasons.Single());
        Assert.Equal("ExampleRejectedEvidence", result.RejectionEvidence.Single().Name);
    }

    [Fact]
    public void Reject_when_should_invert_the_rejection_predicate()
    {
        var admission = BusinessAdmission<ExternalCommand, AcceptedCommand>
            .Create("ExampleAdmission")
            .RejectWhen(
                code: "BoundaryMustReceiveIntentNotStatus",
                description: "The boundary accepts intent, not direct status mutation.",
                shouldReject: command => !string.IsNullOrWhiteSpace(command.RequestedStatus),
                rejectionReason: "Status mutation is not accepted.")
            .AcceptWith(command => new AcceptedCommand(command.CorrelationId))
            .Build();

        var result = admission.Admit(new ExternalCommand("corr-1", RequestedStatus: "Delivered"));

        Assert.False(result.WasAccepted);
        Assert.Equal("Status mutation is not accepted.", result.RejectionReasons.Single());
        Assert.Equal("BoundaryMustReceiveIntentNotStatusRejected", result.RejectionEvidence.Single().Name);
    }

    [Fact]
    public void Build_should_require_accept_function()
    {
        var builder = BusinessAdmission<ExternalCommand, AcceptedCommand>
            .Create("ExampleAdmission")
            .Require(
                code: "CorrelationIdRequired",
                description: "A correlation id is required for traceability.",
                isSatisfied: command => !string.IsNullOrWhiteSpace(command.CorrelationId),
                rejectionReason: "Correlation id is required.");

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("must declare an accept function", exception.Message);
    }

    private sealed record ExternalCommand(string CorrelationId, string? RequestedStatus = null);

    private sealed record AcceptedCommand(string CorrelationId);
}
