namespace TrustableCode.SDK.BusinessModeling.Tests;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equal_value_objects_with_same_components_should_be_equal()
    {
        var left = new Money(10m, "USD");
        var right = new Money(10m, "USD");

        Assert.Equal(left, right);
        Assert.True(left == right);
    }

    [Fact]
    public void Different_value_objects_should_not_be_equal()
    {
        var left = new Money(10m, "USD");
        var right = new Money(20m, "USD");

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }

    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }
}
