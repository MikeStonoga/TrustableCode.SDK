namespace TrustableCode.SDK.BusinessModeling.Identity;

/// <summary>
/// Base type for strongly typed identifiers that prevent primitive confusion at boundaries and in business models.
/// </summary>
public abstract class TypedId<TValue> : ValueObject
    where TValue : notnull
{
    /// <summary>
    /// Creates a strongly typed identifier from the supplied primitive value.
    /// </summary>
    protected TypedId(TValue value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the wrapped identifier value.
    /// </summary>
    public TValue Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value.ToString() ?? string.Empty;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
