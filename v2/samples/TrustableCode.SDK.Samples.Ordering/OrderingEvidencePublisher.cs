using TrustableCode.SDK.TrustableModeling.Evidence;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed class OrderingEvidencePublisher
{
    private readonly BusinessEvidenceRecorder _recorder;

    public OrderingEvidencePublisher(IBusinessEvidenceSink sink)
    {
        _recorder = new BusinessEvidenceRecorder(sink);
    }

    public void Publish(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _recorder.RecordMany(order.BusinessEvidence);
    }
}

