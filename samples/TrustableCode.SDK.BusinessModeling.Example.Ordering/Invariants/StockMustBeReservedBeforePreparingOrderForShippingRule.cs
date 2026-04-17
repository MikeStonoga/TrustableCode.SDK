using TrustableCode.SDK.BusinessModeling.Example.Ordering.Exceptions;
using TrustableCode.SDK.BusinessModeling.Invariants;

namespace TrustableCode.SDK.BusinessModeling.Example.Ordering.Invariants;

public sealed class StockMustBeReservedBeforePreparingOrderForShippingRule(bool stockReserved) : IBusinessInvariantRule
{
    public string Description => "Stock reservation is required before preparing an order for shipping.";

    public void EnsureIsPreserved()
    {
        if (!stockReserved)
        {
            throw new StockMustBeReservedBeforePreparingOrderForShippingException();
        }
    }
}
