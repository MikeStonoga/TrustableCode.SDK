using TrustableCode.SDK.BusinessModeling;
using TrustableCode.SDK.BusinessModeling.Invariants;
using TrustableCode.SDK.BusinessModeling.Observability;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed class Invoice : AggregateRoot
{
    private readonly List<IBusinessEvidence> _businessEvidence = [];

    private Invoice(CreateInvoiceRequirement requirement)
    {
        Id = requirement.InvoiceId;
        CapturedAmount = requirement.CapturedAmount;
        RefundedAmount = Money.Zero(requirement.CapturedAmount.Currency);
        Status = InvoiceStatus.Captured;
    }

    public static BusinessInvariantManifest<InvoiceInvariant> Invariants { get; } =
        BusinessInvariantManifest<InvoiceInvariant>.Create(builder => builder
            .Add(InvoiceInvariant.RefundMustNotExceedCapturedAmount, "Refund must not exceed the captured amount.")
            .Add(InvoiceInvariant.RefundRequiresCapturedInvoice, "Only captured or partially refunded invoices can be refunded."));

    public InvoiceId Id { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public Money CapturedAmount { get; private set; }

    public Money RefundedAmount { get; private set; }

    public bool IsCaptured => Status == InvoiceStatus.Captured;

    public bool IsPartiallyRefunded => Status == InvoiceStatus.PartiallyRefunded;

    public bool IsRefunded => Status == InvoiceStatus.Refunded;

    public static Invoice Create(CreateInvoiceRequirement requirement)
        => new(requirement);

    public InvariantViolationEvidence? LastInvariantViolationEvidence { get; private set; }

    public BusinessTransitionEvidence<InvoiceStatus> Refund(RefundInvoiceRequirement requirement)
    {
        try
        {
            EnsureAll(notification => notification
                .Collect(new RefundRequiresCapturedInvoiceRule(Status))
                .Collect(new RefundMustNotExceedCapturedAmountRule(CapturedAmount, RefundedAmount, requirement.RefundAmount)));
        }
        catch
        {
            LastInvariantViolationEvidence = new InvariantViolationEvidence(
                ModelName: nameof(Invoice),
                InvariantName: nameof(InvoiceInvariant.RefundMustNotExceedCapturedAmount),
                Message: "Refund attempt violated invoice refund protections.",
                CorrelationId: requirement.CorrelationId,
                ObservedAt: requirement.RequestedAt);

            _businessEvidence.Add(LastInvariantViolationEvidence);
            throw;
        }

        var previousStatus = Status;
        RefundedAmount = RefundedAmount + requirement.RefundAmount;
        Status = RefundedAmount.Amount == CapturedAmount.Amount
            ? InvoiceStatus.Refunded
            : InvoiceStatus.PartiallyRefunded;

        RecordBusinessEvent(new InvoiceRefunded(Id, requirement.RefundAmount.Amount, requirement.Reason, requirement.RequestedAt));

        var evidence = new BusinessTransitionEvidence<InvoiceStatus>(
            ModelName: nameof(Invoice),
            TransitionName: "Refund",
            PreviousState: previousStatus,
            CurrentState: Status,
            CorrelationId: requirement.CorrelationId,
            ObservedAt: requirement.RequestedAt);

        _businessEvidence.Add(evidence);
        return evidence;
    }

    public IReadOnlyList<IBusinessEvidence> DequeueBusinessEvidence()
    {
        var pending = _businessEvidence.ToArray();
        _businessEvidence.Clear();
        return pending;
    }
}
