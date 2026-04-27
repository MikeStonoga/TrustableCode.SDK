using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class PaymentReferenceRequiredPrecondition : TransitionPrecondition<OrderStatus, CapturePaymentRequirement>
{
    public PaymentReferenceRequiredPrecondition()
        : base(
            code: "PaymentReferenceRequired",
            description: "Captured payment must carry a provider reference.",
            isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.PaymentReference),
            rejectionReason: "A payment reference is required after capture.")
    {
    }
}
