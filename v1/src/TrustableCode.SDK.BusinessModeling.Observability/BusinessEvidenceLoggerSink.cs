using Microsoft.Extensions.Logging;

namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Optional adapter that forwards business evidence into <see cref="ILogger"/>.
/// </summary>
public sealed class BusinessEvidenceLoggerSink : IBusinessEvidenceSink
{
    private readonly ILogger<BusinessEvidenceLoggerSink> _logger;

    /// <summary>
    /// Creates a logger-backed sink for business evidence.
    /// </summary>
    public BusinessEvidenceLoggerSink(ILogger<BusinessEvidenceLoggerSink> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task WriteAsync(IBusinessEvidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        _logger.LogInformation(
            "Business evidence captured. Type: {EvidenceType}. Payload: {@Evidence}",
            evidence.EvidenceType,
            evidence);

        return Task.CompletedTask;
    }
}
