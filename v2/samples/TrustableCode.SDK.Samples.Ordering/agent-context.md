# Trustable Context: Order Fulfillment

## Business Purpose
Protects the movement of paid orders into shipment readiness without allowing callers to declare shipment state directly.

## State Model
Authoritative state: `Order.Status`
- PlacedAwaitingPayment (initial): The order was created and is waiting for payment capture.
- PaidAwaitingFulfillment: Payment has been captured and fulfillment can be evaluated.
- FulfilledReadyForShipping: Payment and stock requirements were satisfied and the order can be handed to shipping.
- ShippedWaitingDelivery: The order has shipped and is waiting for delivery confirmation.
- Delivered (terminal): The carrier or customer confirmed delivery.
- Cancelled (terminal): The order was intentionally stopped before shipment.

## Suggested Reading Order
- Start with `Order.Status` and the declared states.
- Read valid transitions before changing state mutation code.
- Read boundary rules before accepting external input.
- Read invariants before adding or relaxing behavior.
- Read side effects and evidence before publishing, logging, tracing, or compensating work.

## Expected State Flow
- External intent -> PlacedAwaitingPayment via `CreateOrder`
- PlacedAwaitingPayment -> PaidAwaitingFulfillment via `CapturePayment`
- PaidAwaitingFulfillment -> FulfilledReadyForShipping via `PrepareForShipping`
- FulfilledReadyForShipping -> ShippedWaitingDelivery via `ShipOrder`
- ShippedWaitingDelivery -> Delivered via `DeliverOrder`
- PlacedAwaitingPayment, PaidAwaitingFulfillment, or FulfilledReadyForShipping -> Cancelled via `Cancel`

## Valid Transitions
- CreateOrder: External intent -> PlacedAwaitingPayment. Creates an order through a factory after the creation boundary admits order intent.
  - Preconditions: `The request must identify the order and customer.`, `At least one order line must be present.`, `External callers may not provide an arbitrary initial status.`
  - Produced events: `OrderCreated`
  - Produced evidence: `OrderCreatedEvidence`
- CapturePayment: PlacedAwaitingPayment -> PaidAwaitingFulfillment. Moves an order into the paid state after payment capture is confirmed.
  - Preconditions: `The payment provider must confirm capture.`, `The order must not be cancelled or shipped.`
  - Produced events: `OrderPaymentCaptured`
  - Produced evidence: `OrderPaymentCapturedEvidence`
- PrepareForShipping: PaidAwaitingFulfillment -> FulfilledReadyForShipping. Moves a paid order into shipment readiness after stock is reserved.
  - Preconditions: `Payment must be captured.`, `Stock must be reserved.`, `The order must not already be shipped.`
  - Produced events: `OrderPreparedForShipping`
  - Produced evidence: `OrderPreparedForShippingEvidence`
- ShipOrder: FulfilledReadyForShipping -> ShippedWaitingDelivery. Moves an order into carrier custody once shipment identity is known.
  - Preconditions: `A carrier must be selected.`, `A tracking code must be assigned.`
  - Produced events: `OrderShipped`
  - Produced evidence: `OrderShippedEvidence`
- DeliverOrder: ShippedWaitingDelivery -> Delivered. Closes fulfillment after proof of delivery is captured.
  - Preconditions: `Proof of delivery must be captured.`
  - Produced events: `OrderDelivered`
  - Produced evidence: `OrderDeliveredEvidence`
- Cancel: PlacedAwaitingPayment, PaidAwaitingFulfillment, or FulfilledReadyForShipping -> Cancelled. Stops the order before it becomes shipped.
  - Preconditions: `Orders waiting for delivery cannot be cancelled.`
  - Produced events: `OrderCancelled`
  - Produced evidence: `OrderCancelledEvidence`

## Invariants
- PaymentCapturedBeforeShipmentPreparation (Critical): An order cannot be prepared for shipping unless payment has already been captured.
- StockReservedBeforeShipmentPreparation (Critical): An order cannot be prepared for shipping unless stock has been reserved.
- ShippedOrdersCannotBeCancelled (Critical): Once an order is shipped, cancellation must be represented through a different business process.
- DeliveryRequiresShipment (Critical): An order cannot be delivered unless it has already been shipped.

## Boundary Rules
- CreateOrderFactory: Admits external order creation intent and produces an order awaiting payment.
  - Admission rules: `The caller must identify order and customer.`, `The caller must provide at least one positive-quantity line.`, `The caller may not provide initial status.`
  - Rejection evidence: `OrderCreationRejectedEvidence`
- CapturePaymentAdmission: Admits payment capture facts without letting callers set fulfillment state.
  - Admission rules: `The caller may confirm payment capture, but may not set the target status directly.`, `Captured payment must carry traceable payment facts.`
  - Rejection evidence: `OrderPaymentCaptureRejectedEvidence`
- PrepareOrderForShippingRequirement: Admits intent to prepare an order for shipping, not arbitrary status mutation.
  - Admission rules: `The caller may request preparation, but may not set the target status directly.`, `Payment and stock facts must come from trusted system state or verified integrations.`
  - Rejection evidence: `OrderPreparationRejectedEvidence`
- ShipOrderAdmission: Admits shipment facts without letting callers mark the order shipped directly.
  - Admission rules: `The caller may confirm shipment, but may not set the target status directly.`, `Carrier and tracking facts must be present for the transition to apply.`
  - Rejection evidence: `OrderShipmentRejectedEvidence`
- DeliverOrderAdmission: Admits delivery evidence without letting callers mark the order delivered directly.
  - Admission rules: `The caller may confirm delivery, but may not set the target status directly.`, `Proof of delivery must be captured for the transition to apply.`
  - Rejection evidence: `OrderDeliveryRejectedEvidence`
- CancelOrderRequirement: Admits intent to cancel an order before shipment.
  - Admission rules: `The request must not bypass the shipped-order invariant.`, `The caller must provide a cancellation reason that can be audited.`
  - Rejection evidence: `OrderCancellationRejectedEvidence`

## Application Entry Points
- OrderingApplicationService: Runs in-memory application operations over an already loaded Order aggregate.
  - Use when: Use for tests, in-memory workflows, or application layers that already loaded the aggregate.
  - Reads: `Order.Status`, `ExternalRequests/*`
  - Writes: `Order.StatusState`, `Order.BusinessEvidence`
  - Emits: `Transition events`, `BusinessEvidence`, `SideEffectLifecycle evidence`
- PersistedOrderingApplicationService: Loads an OrderPersistenceSnapshot, rehydrates an Order, executes governed behavior, saves a new snapshot, and enqueues produced events.
  - Use when: Use when a command targets an order that already exists in persistence.
  - Reads: `IOrderSnapshotStore`, `OrderPersistenceSnapshot`, `ExternalRequests/*`
  - Writes: `IOrderSnapshotStore`, `IOrderingOutbox`
  - Emits: `OrderingOutboxMessage`, `BusinessEvidence`, `SideEffectLifecycle evidence`

## Side Effects
- PersistOrderPreparedEvent: Persist and publish the business event through an outbox in the same commit as the order state change. [TransactionalOutbox; idempotency=True; compensation=False]
- NotifyFulfillment: Notify fulfillment only after the order preparation event is durably recorded. [EventuallyConsistent; idempotency=True; compensation=True]

## Evidence
- OrderCreatedEvidence (Transition): Records order creation through the factory and the initial awaiting-payment state.
- OrderPaymentCapturedEvidence (Transition): Records confirmed payment capture and movement into fulfillment.
- OrderPaymentCaptureRejectedEvidence (BoundaryRejection): Records why payment capture input was rejected before business meaning was admitted.
- OrderPreparedForShippingEvidence (Transition): Records previous and current order state with correlation.
- OrderShippedEvidence (Transition): Records carrier and tracking readiness for shipment.
- OrderShipmentRejectedEvidence (BoundaryRejection): Records why shipment input was rejected before business meaning was admitted.
- OrderDeliveredEvidence (Transition): Records proof-backed delivery completion.
- OrderDeliveryRejectedEvidence (BoundaryRejection): Records why delivery input was rejected before business meaning was admitted.
- OrderPreparationRejectedEvidence (InvariantViolation): Records why shipment preparation was rejected before state changed.
- OrderCancellationRejectedEvidence (BoundaryRejection): Records why cancellation input was rejected before business meaning was admitted.
- OrderFulfillmentNotificationEvidence (SideEffect): Records whether fulfillment notification was sent, skipped, retried, or scheduled for compensation.

## Rejection And Observation Map
- CreateOrderFactory rejects with `OrderCreationRejectedEvidence`
- CapturePaymentAdmission rejects with `OrderPaymentCaptureRejectedEvidence`
- PrepareOrderForShippingRequirement rejects with `OrderPreparationRejectedEvidence`
- ShipOrderAdmission rejects with `OrderShipmentRejectedEvidence`
- DeliverOrderAdmission rejects with `OrderDeliveryRejectedEvidence`
- CancelOrderRequirement rejects with `OrderCancellationRejectedEvidence`

## Change Checklist
- Do not introduce a state change that is missing from Valid Transitions.
- Do not admit external input without a Boundary Rule and rejection evidence.
- Preserve or explicitly replace every listed invariant.
- Keep side effects idempotent when the descriptor requires an idempotency key.
- Emit or preserve declared evidence for accepted, rejected, and side-effect behavior.

## Non-Goals
- Do not replace named transitions with direct status mutation.
- Do not create orders by directly constructing arbitrary status.
- Do not allow external callers to submit an arbitrary target order status.
- Do not publish fulfillment side effects before the business event is durable.
