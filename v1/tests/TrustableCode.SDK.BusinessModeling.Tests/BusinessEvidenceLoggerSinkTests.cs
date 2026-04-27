using Microsoft.Extensions.Logging.Abstractions;
using TrustableCode.SDK.BusinessModeling.Observability;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessEvidenceLoggerSinkTests
{
    [Fact]
    public async Task WriteAsync_accepts_business_evidence()
    {
        var sink = new BusinessEvidenceLoggerSink(new NullLogger<BusinessEvidenceLoggerSink>());
        var evidence = new SideEffectEvidence(
            SideEffectName: "SendInvoiceEmail",
            Outcome: "completed",
            BusinessOperation: "InvoiceIssued",
            CorrelationId: "corr-001",
            ObservedAt: DateTimeOffset.UtcNow);

        var exception = await Record.ExceptionAsync(() => sink.WriteAsync(evidence));

        Assert.Null(exception);
    }
}
