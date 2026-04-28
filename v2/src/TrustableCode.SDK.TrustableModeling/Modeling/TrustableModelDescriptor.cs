namespace TrustableCode.SDK.TrustableModeling.Modeling;

/// <summary>
/// Describes the semantic safety envelope for an important business model.
/// </summary>
public sealed class TrustableModelDescriptor
{
    private TrustableModelDescriptor(Builder builder)
    {
        Name = Require.Text(builder.Name, nameof(Name));
        BusinessPurpose = Require.Text(builder.BusinessPurpose, nameof(BusinessPurpose));
        StateModel = builder.StateModel ?? throw new InvalidOperationException("A trustable model requires an explicit state model.");
        Transitions = builder.Transitions.ToArray();
        Invariants = builder.Invariants.ToArray();
        Boundaries = builder.Boundaries.ToArray();
        ApplicationEntryPoints = builder.ApplicationEntryPoints.ToArray();
        SideEffects = builder.SideEffects.ToArray();
        Evidence = builder.Evidence.ToArray();
        NonGoals = builder.NonGoals.ToArray();
    }

    public string Name { get; }

    public string BusinessPurpose { get; }

    public StateModel StateModel { get; }

    public IReadOnlyList<BusinessTransitionDescriptor> Transitions { get; }

    public IReadOnlyList<InvariantDescriptor> Invariants { get; }

    public IReadOnlyList<BoundaryContract> Boundaries { get; }

    public IReadOnlyList<ApplicationEntryPointDescriptor> ApplicationEntryPoints { get; }

    public IReadOnlyList<SideEffectContract> SideEffects { get; }

    public IReadOnlyList<EvidenceContract> Evidence { get; }

    public IReadOnlyList<string> NonGoals { get; }

    public static TrustableModelDescriptor Create(Action<Builder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new Builder();
        configure(builder);
        return new TrustableModelDescriptor(builder);
    }

    public sealed class Builder
    {
        internal string? Name { get; private set; }

        internal string? BusinessPurpose { get; private set; }

        internal StateModel? StateModel { get; private set; }

        internal List<BusinessTransitionDescriptor> Transitions { get; } = [];

        internal List<InvariantDescriptor> Invariants { get; } = [];

        internal List<BoundaryContract> Boundaries { get; } = [];

        internal List<ApplicationEntryPointDescriptor> ApplicationEntryPoints { get; } = [];

        internal List<SideEffectContract> SideEffects { get; } = [];

        internal List<EvidenceContract> Evidence { get; } = [];

        internal List<string> NonGoals { get; } = [];

        public Builder Describe(string name, string businessPurpose)
        {
            Name = Require.Text(name, nameof(name));
            BusinessPurpose = Require.Text(businessPurpose, nameof(businessPurpose));
            return this;
        }

        public Builder WithStateModel(StateModel stateModel)
        {
            StateModel = stateModel ?? throw new ArgumentNullException(nameof(stateModel));
            return this;
        }

        public Builder AddTransition(BusinessTransitionDescriptor transition)
        {
            Transitions.Add(transition ?? throw new ArgumentNullException(nameof(transition)));
            return this;
        }

        public Builder AddInvariant(InvariantDescriptor invariant)
        {
            Invariants.Add(invariant ?? throw new ArgumentNullException(nameof(invariant)));
            return this;
        }

        public Builder AddBoundary(BoundaryContract boundary)
        {
            Boundaries.Add(boundary ?? throw new ArgumentNullException(nameof(boundary)));
            return this;
        }

        public Builder AddApplicationEntryPoint(ApplicationEntryPointDescriptor entryPoint)
        {
            ApplicationEntryPoints.Add(entryPoint ?? throw new ArgumentNullException(nameof(entryPoint)));
            return this;
        }

        public Builder AddSideEffect(SideEffectContract sideEffect)
        {
            SideEffects.Add(sideEffect ?? throw new ArgumentNullException(nameof(sideEffect)));
            return this;
        }

        public Builder AddEvidence(EvidenceContract evidence)
        {
            Evidence.Add(evidence ?? throw new ArgumentNullException(nameof(evidence)));
            return this;
        }

        public Builder AddNonGoal(string nonGoal)
        {
            NonGoals.Add(Require.Text(nonGoal, nameof(nonGoal)));
            return this;
        }
    }
}
