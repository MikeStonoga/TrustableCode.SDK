using TrustableCode.SDK.BusinessModeling.Exceptions;
using TrustableCode.SDK.BusinessModeling.Example.Invoicing;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class InvoicingSampleTests
{
    [Fact]
    public void Refund_should_preserve_invariant_and_emit_transition()
    {
        var invoice = Invoice.Create(new CreateInvoiceRequirement(
            InvoiceId.New(),
            new Money(100m, "USD")));

        var evidence = invoice.Refund(new RefundInvoiceRequirement(
            RefundAmount: new Money(40m, "USD"),
            Reason: "Customer request",
            RequestedAt: new DateTimeOffset(2026, 4, 17, 22, 0, 0, TimeSpan.Zero),
            CorrelationId: "corr-invoice-1"));

        Assert.True(invoice.IsPartiallyRefunded);
        Assert.Equal(40m, invoice.RefundedAmount.Amount);
        Assert.Equal(InvoiceStatus.Captured, evidence.PreviousState);
        Assert.Equal(InvoiceStatus.PartiallyRefunded, evidence.CurrentState);
    }

    [Fact]
    public void Refund_should_fail_when_requested_amount_exceeds_captured_amount()
    {
        var invoice = Invoice.Create(new CreateInvoiceRequirement(
            InvoiceId.New(),
            new Money(100m, "USD")));

        var exception = Assert.Throws<AggregatedBusinessRuleViolationException>(() =>
            invoice.Refund(new RefundInvoiceRequirement(
                RefundAmount: new Money(120m, "USD"),
                Reason: "Invalid refund",
                RequestedAt: DateTimeOffset.UtcNow,
                CorrelationId: "corr-invoice-2")));

        Assert.Contains("Refund must not exceed the captured amount.", exception.Violations);
        Assert.NotNull(invoice.LastInvariantViolationEvidence);
    }
}
