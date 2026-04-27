using TrustableCode.SDK.TrustableModeling.Invariants;
using TrustableCode.SDK.TrustableModeling.Modeling;
using TrustableCode.SDK.TrustableModeling.Transitions;

namespace TrustableCode.SDK.Samples.Ordering;

public static class OrderFulfillmentInvariants
{
    public static IBusinessInvariant<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>> PaymentCapturedBeforeShipmentPreparation { get; } =
        new BusinessInvariant<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>>(
            descriptor: new InvariantDescriptor(
                code: "PaymentCapturedBeforeShipmentPreparation",
                name: "Payment captured before shipment preparation",
                description: "An order cannot be prepared for shipping unless payment has already been captured."),
            isPreserved: context => context.Input.PaymentCaptured,
            violationMessage: "Payment must be captured before the order can be prepared for shipping.");

    public static IBusinessInvariant<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>> StockReservedBeforeShipmentPreparation { get; } =
        new BusinessInvariant<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>>(
            descriptor: new InvariantDescriptor(
                code: "StockReservedBeforeShipmentPreparation",
                name: "Stock reserved before shipment preparation",
                description: "An order cannot be prepared for shipping unless stock has been reserved."),
            isPreserved: context => context.Input.StockReserved,
            violationMessage: "Stock must be reserved before the order can be prepared for shipping.");

    public static IReadOnlyList<IBusinessInvariant<TransitionContext<OrderStatus, PrepareOrderForShippingRequirement>>> PrepareForShipping { get; } =
    [
        PaymentCapturedBeforeShipmentPreparation,
        StockReservedBeforeShipmentPreparation
    ];
}

