namespace TrustableCode.SDK.BusinessModeling.Example.Ordering;

public enum OrderInvariant
{
    PaymentMustBeCapturedBeforeShipping = 1,
    StockMustBeReservedBeforeShipping = 2,
    OnlyPaidOrdersCanBePreparedForShipping = 3,
    ShippedOrdersCannotBeCancelled = 4
}
