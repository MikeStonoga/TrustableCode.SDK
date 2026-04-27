using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class ProofOfDeliveryRequiredPrecondition : TransitionPrecondition<OrderStatus, DeliverOrderRequirement>
{
    public ProofOfDeliveryRequiredPrecondition()
        : base(
            code: "ProofOfDeliveryRequired",
            description: "Delivery must be backed by carrier or customer confirmation.",
            isSatisfied: (_, requirement) => requirement.ProofOfDeliveryCaptured,
            rejectionReason: "Proof of delivery is required before the order can be delivered.")
    {
    }
}
