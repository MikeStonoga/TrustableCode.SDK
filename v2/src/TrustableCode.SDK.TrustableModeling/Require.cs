namespace TrustableCode.SDK.TrustableModeling;

internal static class Require
{
    internal static string Text(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value;
    }

    internal static IReadOnlyList<T> NotEmpty<T>(IEnumerable<T>? values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);

        var materialized = values.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("The collection must contain at least one item.", parameterName);
        }

        return materialized;
    }

    internal static IReadOnlyList<string> TextList(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        return values
            .Select(value => Text(value, nameof(values)))
            .ToArray();
    }
}

