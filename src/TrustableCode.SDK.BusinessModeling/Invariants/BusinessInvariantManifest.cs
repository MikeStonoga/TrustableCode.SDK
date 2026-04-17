namespace TrustableCode.SDK.BusinessModeling.Invariants;

/// <summary>
/// Exposes a discoverable manifest of important business invariants for a model.
/// </summary>
public sealed class BusinessInvariantManifest<TInvariant>
    where TInvariant : notnull
{
    private readonly IReadOnlyDictionary<TInvariant, string> _descriptions;

    private BusinessInvariantManifest(IReadOnlyDictionary<TInvariant, string> descriptions)
    {
        _descriptions = descriptions;
    }

    /// <summary>
    /// Gets the invariant descriptions keyed by their semantic name.
    /// </summary>
    public IReadOnlyDictionary<TInvariant, string> Descriptions => _descriptions;

    /// <summary>
    /// Gets the description for a specific invariant key.
    /// </summary>
    public string this[TInvariant invariant] => _descriptions[invariant];

    /// <summary>
    /// Tries to get the description for a specific invariant key.
    /// </summary>
    public bool TryGetDescription(TInvariant invariant, out string? description)
        => _descriptions.TryGetValue(invariant, out description);

    /// <summary>
    /// Creates an invariant manifest using a builder callback.
    /// </summary>
    public static BusinessInvariantManifest<TInvariant> Create(Action<Builder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new Builder();
        configure(builder);
        return new BusinessInvariantManifest<TInvariant>(builder.Build());
    }

    /// <summary>
    /// Builder for composing a business invariant manifest.
    /// </summary>
    public sealed class Builder
    {
        private readonly Dictionary<TInvariant, string> _descriptions = [];

        /// <summary>
        /// Adds a business invariant description under the given semantic key.
        /// </summary>
        public Builder Add(TInvariant invariant, string description)
        {
            ArgumentNullException.ThrowIfNull(invariant);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            _descriptions[invariant] = description;
            return this;
        }

        internal IReadOnlyDictionary<TInvariant, string> Build()
            => new Dictionary<TInvariant, string>(_descriptions);
    }
}
