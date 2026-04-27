namespace TrustableCode.SDK.BusinessModeling.Observability;

/// <summary>
/// Destination for business evidence. Adapters can forward evidence to logs, tracing, metrics, or audit stores.
/// </summary>
public interface IBusinessEvidenceSink
{
    /// <summary>
    /// Writes business evidence to an external sink.
    /// </summary>
    Task WriteAsync(IBusinessEvidence evidence, CancellationToken cancellationToken = default);
}
