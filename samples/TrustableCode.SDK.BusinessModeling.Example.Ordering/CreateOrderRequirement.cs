namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public sealed record CreateOrderRequirement(
    OrderId OrderId,
    OrderStatus InitialStatus = OrderStatus.Draft);
