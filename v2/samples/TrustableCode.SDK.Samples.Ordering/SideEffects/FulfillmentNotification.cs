namespace TrustableCode.SDK.Samples.Ordering.SideEffects;

public sealed record FulfillmentNotification(string OrderId, string CorrelationId);

