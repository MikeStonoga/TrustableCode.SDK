using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering.Transitions;

public sealed class CarrierRequiredPrecondition : TransitionPrecondition<OrderStatus, ShipOrderRequirement>
{
    public CarrierRequiredPrecondition()
        : base(
            code: "CarrierRequired",
            description: "A shipment must identify the carrier responsible for transport.",
            isSatisfied: (_, requirement) => !string.IsNullOrWhiteSpace(requirement.Carrier),
            rejectionReason: "A carrier is required before the order can be shipped.")
    {
    }
}
