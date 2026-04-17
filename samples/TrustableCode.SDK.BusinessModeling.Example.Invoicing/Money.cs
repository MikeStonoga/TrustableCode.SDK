using TrustableCode.SDK.BusinessModeling;

namespace TrustableCode.SDK.BusinessModeling.Example.Invoicing;

public sealed class Money(decimal amount, string currency) : ValueObject
{
    public decimal Amount { get; } = amount;

    public string Currency { get; } = currency;

    public static Money Zero(string currency) => new(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException("Money values must use the same currency.");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
