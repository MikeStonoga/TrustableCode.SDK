using TrustableCode.SDK.TrustableModeling.Modeling;

namespace TrustableCode.SDK.Samples.Ordering;

/// <summary>
/// Semantic safety envelope for the order fulfillment sample.
/// </summary>
public static class OrderFulfillmentTrustableModel
{
    /// <summary>
    /// Describes the state, transitions, invariants, boundaries, side effects, and evidence
    /// that must guide changes to order fulfillment.
    /// </summary>
    public static TrustableModelDescriptor Descriptor { get; } =
        TrustableModelDescriptor.Create(builder => builder
            .Describe(
                name: "Order Fulfillment",
                businessPurpose: "Protects the movement of paid orders into shipment readiness without allowing callers to declare shipment state directly.")
            .WithStateModel(new StateModel(
                authoritativeState: "Order.Status",
                states:
                [
                    new StateDefinition("Pending", "The order exists but payment has not been captured.", isInitial: true),
                    new StateDefinition("Paid", "Payment has been captured and fulfillment can be evaluated."),
                    new StateDefinition("ReadyForShipping", "Payment and stock requirements were satisfied and the order can be handed to shipping."),
                    new StateDefinition("Shipped", "The order has left fulfillment.", isTerminal: true),
                    new StateDefinition("Cancelled", "The order was intentionally stopped before shipment.", isTerminal: true)
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "CapturePayment",
                fromState: "Pending",
                toState: "Paid",
                description: "Moves an order into the paid state after payment capture is confirmed.",
                preconditions:
                [
                    "The payment provider must confirm capture.",
                    "The order must not be cancelled or shipped."
                ],
                producedEvents:
                [
                    "OrderPaymentCaptured"
                ],
                producedEvidence:
                [
                    "OrderPaymentCapturedEvidence"
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "PrepareForShipping",
                fromState: "Paid",
                toState: "ReadyForShipping",
                description: "Moves a paid order into shipment readiness after stock is reserved.",
                preconditions:
                [
                    "Payment must be captured.",
                    "Stock must be reserved.",
                    "The order must not already be shipped."
                ],
                producedEvents:
                [
                    "OrderPreparedForShipping"
                ],
                producedEvidence:
                [
                    "OrderPreparedForShippingEvidence"
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "Cancel",
                fromState: "Pending or Paid",
                toState: "Cancelled",
                description: "Stops the order before it becomes shipped.",
                preconditions:
                [
                    "Shipped orders cannot be cancelled."
                ],
                producedEvents:
                [
                    "OrderCancelled"
                ],
                producedEvidence:
                [
                    "OrderCancelledEvidence"
                ]))
            .AddInvariant(new InvariantDescriptor(
                code: "PaymentCapturedBeforeShipmentPreparation",
                name: "Payment captured before shipment preparation",
                description: "An order cannot be prepared for shipping unless payment has already been captured."))
            .AddInvariant(new InvariantDescriptor(
                code: "StockReservedBeforeShipmentPreparation",
                name: "Stock reserved before shipment preparation",
                description: "An order cannot be prepared for shipping unless stock has been reserved."))
            .AddInvariant(new InvariantDescriptor(
                code: "ShippedOrdersCannotBeCancelled",
                name: "Shipped orders cannot be cancelled",
                description: "Once an order is shipped, cancellation must be represented through a different business process."))
            .AddBoundary(new BoundaryContract(
                name: "PrepareOrderForShippingRequirement",
                description: "Admits intent to prepare an order for shipping, not arbitrary status mutation.",
                admissionRules:
                [
                    "The caller may request preparation, but may not set the target status directly.",
                    "Payment and stock facts must come from trusted system state or verified integrations."
                ],
                rejectionEvidence:
                [
                    "OrderPreparationRejectedEvidence"
                ]))
            .AddBoundary(new BoundaryContract(
                name: "CancelOrderRequirement",
                description: "Admits intent to cancel an order before shipment.",
                admissionRules:
                [
                    "The request must not bypass the shipped-order invariant.",
                    "The caller must provide a cancellation reason that can be audited."
                ],
                rejectionEvidence:
                [
                    "OrderCancellationRejectedEvidence"
                ]))
            .AddSideEffect(new SideEffectContract(
                name: "PersistOrderPreparedEvent",
                description: "Persist and publish the business event through an outbox in the same commit as the order state change.",
                consistency: SideEffectConsistency.TransactionalOutbox,
                requiresIdempotencyKey: true))
            .AddSideEffect(new SideEffectContract(
                name: "NotifyFulfillment",
                description: "Notify fulfillment only after the order preparation event is durably recorded.",
                consistency: SideEffectConsistency.EventuallyConsistent,
                requiresIdempotencyKey: true,
                requiresCompensation: true))
            .AddEvidence(new EvidenceContract(
                name: "OrderPreparedForShippingEvidence",
                kind: EvidenceKind.Transition,
                description: "Records previous and current order state with correlation."))
            .AddEvidence(new EvidenceContract(
                name: "OrderPreparationRejectedEvidence",
                kind: EvidenceKind.InvariantViolation,
                description: "Records why shipment preparation was rejected before state changed."))
            .AddEvidence(new EvidenceContract(
                name: "OrderFulfillmentNotificationEvidence",
                kind: EvidenceKind.SideEffect,
                description: "Records whether fulfillment notification was sent, skipped, retried, or scheduled for compensation."))
            .AddNonGoal("Do not replace named transitions with direct status mutation.")
            .AddNonGoal("Do not allow external callers to submit an arbitrary target order status.")
            .AddNonGoal("Do not publish fulfillment side effects before the business event is durable."));
}

