namespace TrustableCode.SDK.Samples.Ordering;

public sealed record OrderingOutboxMessage(
    string StreamName,
    string EventName,
    string OrderId,
    string CorrelationId);
