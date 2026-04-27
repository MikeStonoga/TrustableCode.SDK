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
                    new StateDefinition("AwaitingPayment", "The order was created and is waiting for payment capture.", isInitial: true),
                    new StateDefinition("PaidAwaitingFulfillment", "Payment has been captured and fulfillment can be evaluated."),
                    new StateDefinition("ReadyForShipping", "Payment and stock requirements were satisfied and the order can be handed to shipping."),
                    new StateDefinition("Shipped", "The order has left fulfillment and is in carrier custody."),
                    new StateDefinition("Delivered", "The carrier or customer confirmed delivery.", isTerminal: true),
                    new StateDefinition("Cancelled", "The order was intentionally stopped before shipment.", isTerminal: true)
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "CreateOrder",
                fromState: "External intent",
                toState: "AwaitingPayment",
                description: "Creates an order through a factory after the creation boundary admits order intent.",
                preconditions:
                [
                    "The request must identify the order and customer.",
                    "At least one order line must be present.",
                    "External callers may not provide an arbitrary initial status."
                ],
                producedEvents:
                [
                    "OrderCreated"
                ],
                producedEvidence:
                [
                    "OrderCreatedEvidence"
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "CapturePayment",
                fromState: "AwaitingPayment",
                toState: "PaidAwaitingFulfillment",
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
                fromState: "PaidAwaitingFulfillment",
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
                name: "ShipOrder",
                fromState: "ReadyForShipping",
                toState: "Shipped",
                description: "Moves an order into carrier custody once shipment identity is known.",
                preconditions:
                [
                    "A carrier must be selected.",
                    "A tracking code must be assigned."
                ],
                producedEvents:
                [
                    "OrderShipped"
                ],
                producedEvidence:
                [
                    "OrderShippedEvidence"
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "DeliverOrder",
                fromState: "Shipped",
                toState: "Delivered",
                description: "Closes fulfillment after proof of delivery is captured.",
                preconditions:
                [
                    "Proof of delivery must be captured."
                ],
                producedEvents:
                [
                    "OrderDelivered"
                ],
                producedEvidence:
                [
                    "OrderDeliveredEvidence"
                ]))
            .AddTransition(new BusinessTransitionDescriptor(
                name: "Cancel",
                fromState: "AwaitingPayment, PaidAwaitingFulfillment, or ReadyForShipping",
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
            .AddInvariant(new InvariantDescriptor(
                code: "DeliveryRequiresShipment",
                name: "Delivery requires shipment",
                description: "An order cannot be delivered unless it has already been shipped."))
            .AddBoundary(new BoundaryContract(
                name: "CreateOrderFactory",
                description: "Admits external order creation intent and produces an order awaiting payment.",
                admissionRules:
                [
                    "The caller must identify order and customer.",
                    "The caller must provide at least one positive-quantity line.",
                    "The caller may not provide initial status."
                ],
                rejectionEvidence:
                [
                    "OrderCreationRejectedEvidence"
                ]))
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
                name: "OrderCreatedEvidence",
                kind: EvidenceKind.Transition,
                description: "Records order creation through the factory and the initial awaiting-payment state."))
            .AddEvidence(new EvidenceContract(
                name: "OrderPaymentCapturedEvidence",
                kind: EvidenceKind.Transition,
                description: "Records confirmed payment capture and movement into fulfillment."))
            .AddEvidence(new EvidenceContract(
                name: "OrderPreparedForShippingEvidence",
                kind: EvidenceKind.Transition,
                description: "Records previous and current order state with correlation."))
            .AddEvidence(new EvidenceContract(
                name: "OrderShippedEvidence",
                kind: EvidenceKind.Transition,
                description: "Records carrier and tracking readiness for shipment."))
            .AddEvidence(new EvidenceContract(
                name: "OrderDeliveredEvidence",
                kind: EvidenceKind.Transition,
                description: "Records proof-backed delivery completion."))
            .AddEvidence(new EvidenceContract(
                name: "OrderPreparationRejectedEvidence",
                kind: EvidenceKind.InvariantViolation,
                description: "Records why shipment preparation was rejected before state changed."))
            .AddEvidence(new EvidenceContract(
                name: "OrderFulfillmentNotificationEvidence",
                kind: EvidenceKind.SideEffect,
                description: "Records whether fulfillment notification was sent, skipped, retried, or scheduled for compensation."))
            .AddNonGoal("Do not replace named transitions with direct status mutation.")
            .AddNonGoal("Do not create orders by directly constructing arbitrary status.")
            .AddNonGoal("Do not allow external callers to submit an arbitrary target order status.")
            .AddNonGoal("Do not publish fulfillment side effects before the business event is durable."));
}
