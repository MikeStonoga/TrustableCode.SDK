namespace TrustableCode.SDK.Samples.Ordering;

public enum OrderStatus
{
    PlacedAwaitingPayment = 0,
    PaidAwaitingFulfillment = 1,
    FulfilledReadyForShipping = 2,
    ShippedWaitingDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}
