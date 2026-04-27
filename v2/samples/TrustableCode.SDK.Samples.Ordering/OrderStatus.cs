namespace TrustableCode.SDK.Samples.Ordering;

public enum OrderStatus
{
    AwaitingPayment = 0,
    PaidAwaitingFulfillment = 1,
    ReadyForShipping = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
