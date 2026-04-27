using TrustableCode.SDK.BusinessModeling;
using TrustableCode.SDK.BusinessModeling.Compensation;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed class InvoiceApproval : AggregateRoot
{
    private readonly List<IBusinessEvidence> _businessEvidence = [];

    private InvoiceApproval(CreateInvoiceApprovalRequirement requirement)
    {
        Id = requirement.InvoiceApprovalId;
        ApprovedAt = requirement.ApprovedAt;
        Status = InvoiceApprovalStatus.Approved;
    }

    public InvoiceApprovalId Id { get; }

    public DateTimeOffset ApprovedAt { get; private set; }

    public InvoiceApprovalStatus Status { get; private set; }

    public string? DownstreamRejectionReason { get; private set; }

    public CompensationRequest<InvoiceApprovalId>? LastCompensationRequest { get; private set; }

    public bool IsApproved => Status == InvoiceApprovalStatus.Approved;

    public bool IsApprovalCompensationPending => Status == InvoiceApprovalStatus.ApprovalCompensationPending;

    public IReadOnlyList<IBusinessEvidence> BusinessEvidence => _businessEvidence;

    public static InvoiceApproval Create(CreateInvoiceApprovalRequirement requirement)
        => new(requirement);

    public InvoiceApprovalCompensationResult ScheduleApprovalCompensation(ScheduleInvoiceApprovalCompensationRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(requirement);

        var decision = InvoiceApprovalCompensationDecision.Create(requirement);
        var executedTransition = new ScheduleInvoiceApprovalCompensationTransition(
            currentStateAccessor: () => Status,
            apply: next => Status = next)
            .Execute();

        DownstreamRejectionReason = decision.DownstreamRejectionReason;
        LastCompensationRequest = CompensationRequest<InvoiceApprovalId>.CreateFor(
            subjectId: Id,
            compensationName: executedTransition.Name,
            decision: decision.Compensation);

        RecordBusinessEvent(new InvoiceApprovalCompensationScheduled(
            Id,
            decision.CompensationId,
            decision.DownstreamRejectionReason,
            requirement.RequestedAt));

        var transitionEvidence = new InvoiceApprovalCompensationScheduledEvidence(
            PreviousState: executedTransition.From,
            CurrentState: executedTransition.To,
            CorrelationId: requirement.CorrelationId,
            ObservedAt: requirement.RequestedAt);

        var compensationEvidence = new CompensationScheduledEvidence(
            ModelName: nameof(InvoiceApproval),
            CompensationName: executedTransition.Name,
            CompensationId: decision.CompensationId.ToString(),
            Reason: decision.DownstreamRejectionReason,
            CorrelationId: requirement.CorrelationId,
            ObservedAt: requirement.RequestedAt);

        _businessEvidence.Add(transitionEvidence);
        _businessEvidence.Add(compensationEvidence);

        return new InvoiceApprovalCompensationResult(
            LastCompensationRequest,
            transitionEvidence,
            compensationEvidence);
    }

    public IReadOnlyList<IBusinessEvidence> DequeueBusinessEvidence()
    {
        var pending = _businessEvidence.ToArray();
        _businessEvidence.Clear();
        return pending;
    }
}
