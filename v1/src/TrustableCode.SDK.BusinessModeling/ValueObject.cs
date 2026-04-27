namespace TrustableCode.SDK.BusinessModeling;

/// <summary>
/// Base type for immutable concepts whose equality is defined by their components rather than identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the components that define value equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ValueObject other
        && GetType() == other.GetType()
        && Equals(other);

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null || GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Compares two value objects for equality.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    /// <summary>
    /// Compares two value objects for inequality.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
