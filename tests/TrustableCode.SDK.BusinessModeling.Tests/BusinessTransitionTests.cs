using TrustableCode.SDK.BusinessModeling.Transitions;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class BusinessTransitionTests
{
    [Fact]
    public void BusinessTransition_preserves_named_direction()
    {
        var transition = new BusinessTransition<OrderStatus>(
            Name: "PrepareForShipping",
            From: OrderStatus.Paid,
            To: OrderStatus.ReadyForShipping);

        Assert.Equal("PrepareForShipping", transition.Name);
        Assert.Equal(OrderStatus.Paid, transition.From);
        Assert.Equal(OrderStatus.ReadyForShipping, transition.To);
    }

    private enum OrderStatus
    {
        Paid = 1,
        ReadyForShipping = 2
    }
}
