using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class TrackingCodeRequiredPrecondition : TransitionPrecondition<OrderStatus, ShipOrderRequirement>
{
    public TrackingCodeRequiredPrecondition()
        : base(
            code: "TrackingCodeRequired",
            description: "A shipment must be traceable after it leaves fulfillment.",
            isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.TrackingCode),
            rejectionReason: "A tracking code is required before the order can be shipped.")
    {
    }
}
