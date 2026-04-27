using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class CancellationReasonRequiredPrecondition : TransitionPrecondition<OrderStatus, CancelOrderRequirement>
{
    public CancellationReasonRequiredPrecondition()
        : base(
            code: "ReasonRequired",
            description: "Cancellation must be auditable.",
            isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.Reason),
            rejectionReason: "A cancellation reason is required.")
    {
    }
}
