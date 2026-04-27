using TrustableCode.SDK.TrustableModeling.Evidence;

namespace TrustableCode.SDK.Samples.Ordering;

public sealed class OrderingEvidencePublisher
{
    private readonly BusinessEvidenceRecorder _recorder;
    private readonly Dictionary<Order, int> _publishedCountsByOrder = [];

    public OrderingEvidencePublisher(IBusinessEvidenceSink sink)
    {
        _recorder = new BusinessEvidenceRecorder(sink);
    }

    public void Publish(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var alreadyPublished = _publishedCountsByOrder.GetValueOrDefault(order);
        _recorder.RecordMany(order.BusinessEvidence.Skip(alreadyPublished));
        _publishedCountsByOrder[order] = order.BusinessEvidence.Count;
    }
}
