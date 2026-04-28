using System.Text.Json;
using TrustableCode.SDK.TrustableModeling.Evidence;

namespace TrustableCode.SDK.Samples.Ordering.Api.Persistence;

public sealed class EfBusinessEvidenceSink(OrderingDbContext db) : IBusinessEvidenceSink
{
    public void Record(BusinessEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        db.BusinessEvidence.Add(new BusinessEvidenceEntity
        {
            Name = evidence.Name,
            Kind = evidence.Kind.ToString(),
            Message = evidence.Message,
            CorrelationId = evidence.CorrelationId,
            ObservedAt = evidence.ObservedAt,
            MetadataJson = JsonSerializer.Serialize(evidence.Metadata)
        });

        db.SaveChanges();
    }
}
