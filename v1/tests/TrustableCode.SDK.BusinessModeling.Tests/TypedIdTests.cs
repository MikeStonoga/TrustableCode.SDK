using TrustableCode.SDK.BusinessModeling.Identity;

namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class TypedIdTests
{
    [Fact]
    public void Typed_ids_with_same_value_should_be_equal()
    {
        var left = new OrderId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var right = new OrderId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.Equal(left, right);
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", left.ToString());
    }

    [Fact]
    public void Different_typed_id_types_should_not_be_equal_even_when_the_value_matches()
    {
        var orderId = new OrderId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var customerId = new CustomerId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.False(orderId.Equals(customerId));
    }

    private sealed class OrderId(Guid value) : TypedId<Guid>(value);

    private sealed class CustomerId(Guid value) : TypedId<Guid>(value);
}
