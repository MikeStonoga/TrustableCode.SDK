# Ordering Sample Class Reference

This file explains the role of each class in the ordering sample and the intended SDK usage path.

## Recommended Flow

External input should not mutate `Order.Status` directly. The sample follows this path:

1. Receive an `ExternalRequests/*` record from the outside world.
2. Pass it through an `Admissions/*Admission` boundary.
3. Convert accepted input into a `Requirements/*Requirement` record.
4. Call an `Order` method that delegates to a specialized transition.
5. Let `GovernedTransition` evaluate state, preconditions, invariants, events, and evidence.
6. Publish `BusinessEvidence` through `OrderingEvidencePublisher` or another sink adapter.
7. Advance side-effect lifecycle when external work must be planned, persisted, published, or compensated.

## Root Model

`OrderingApplicationService`

The recommended application-layer entry point for the sample. It shows the practical SDK flow: receive external input, run admission through `TrustableAdmissionFlow`, call the aggregate transition, publish evidence, and advance side-effect lifecycle with `PlanPersistAndPublish` when shipment preparation succeeds.

`OrderingApplicationResult`

Small result returned by `OrderingApplicationService`. It exposes whether admission passed, transition status, current order status, rejection reasons, produced events/evidence, and the optional side-effect lifecycle record.

`Order`

The executable aggregate in the sample. It owns `Status`, records produced event names, records evidence names, and exposes methods such as `CapturePayment`, `PrepareForShipping`, `Ship`, `Deliver`, and `Cancel`. Each method delegates to a specialized transition instead of changing status inline.

`OrderFactory`

The creation boundary for new orders. It accepts `ExternalCreateOrderRequest`, applies admission rules, creates an `Order` in `PlacedAwaitingPayment`, and records creation evidence. New orders should go through this factory.

`OrderStatus`

The authoritative state model for the aggregate: `PlacedAwaitingPayment`, `PaidAwaitingFulfillment`, `FulfilledReadyForShipping`, `ShippedWaitingDelivery`, `Delivered`, and `Cancelled`.

`OrderLine`

Small value record used by order creation to keep line meaning explicit.

`OrderFulfillmentTrustableModel`

The semantic descriptor for the sample. It declares states, valid transitions, invariants, boundary rules, side effects, evidence, and non-goals for humans and agents before they change code.

## ExternalRequests

`ExternalCreateOrderRequest`

Raw external input for creating an order. It intentionally includes `RequestedStatus` so the sample can show the boundary rejecting arbitrary initial status.

`ExternalCapturePaymentRequest`

Raw external input that says payment was captured. It may not set a target order status directly.

`ExternalPrepareOrderForShippingRequest`

Raw external input requesting shipment preparation. It carries payment and stock facts plus a rejected direct-status field.

`ExternalShipOrderRequest`

Raw external input that confirms shipment facts: carrier and tracking code.

`ExternalDeliverOrderRequest`

Raw external input that confirms delivery evidence and delivery time.

`ExternalCancelOrderRequest`

Raw external input requesting cancellation with an auditable reason.

## Admissions

`CapturePaymentAdmission`

Builds a `BusinessAdmission<ExternalCapturePaymentRequest, CapturePaymentRequirement>`. It rejects direct status mutation and missing correlation before payment facts enter the domain.

`PrepareOrderForShippingAdmission`

Builds a `BusinessAdmission<ExternalPrepareOrderForShippingRequest, PrepareOrderForShippingRequirement>`. It admits shipment-preparation intent while rejecting arbitrary target status.

`ShipOrderAdmission`

Builds a `BusinessAdmission<ExternalShipOrderRequest, ShipOrderRequirement>`. It admits shipment facts without letting the caller mark the order shipped directly.

`DeliverOrderAdmission`

Builds a `BusinessAdmission<ExternalDeliverOrderRequest, DeliverOrderRequirement>`. It admits proof-backed delivery evidence without direct state mutation.

`CancelOrderAdmission`

Builds a `BusinessAdmission<ExternalCancelOrderRequest, CancelOrderRequirement>`. It admits cancellation intent and keeps status decisions inside the governed transition.

## Requirements

`OrderCreationRequirement`

Represents admitted order creation meaning. It documents the intended shape even though creation currently returns the aggregate through `OrderFactory`.

`CapturePaymentRequirement`

Admitted command for payment capture. It carries the capture flag, provider reference, and correlation id used by the transition.

`PrepareOrderForShippingRequirement`

Admitted command for shipment preparation. It carries payment and stock facts checked by invariants.

`ShipOrderRequirement`

Admitted command for shipment. It carries carrier, tracking code, and correlation id.

`DeliverOrderRequirement`

Admitted command for delivery. It carries proof of delivery, delivery timestamp, and correlation id.

`CancelOrderRequirement`

Admitted command for cancellation. It carries the reason and correlation id.

## Transitions

`CapturePaymentTransition`

Domain wrapper around `GovernedTransition<OrderStatus, CapturePaymentRequirement>`. It moves `PlacedAwaitingPayment` to `PaidAwaitingFulfillment` and emits payment evidence.

`PrepareOrderForShippingTransition`

Domain wrapper around `GovernedTransition<OrderStatus, PrepareOrderForShippingRequirement>`. It moves `PaidAwaitingFulfillment` to `FulfilledReadyForShipping` only when the fulfillment invariants pass.

`ShipOrderTransition`

Domain wrapper around `GovernedTransition<OrderStatus, ShipOrderRequirement>`. It moves `FulfilledReadyForShipping` to `ShippedWaitingDelivery` after carrier and tracking facts are present.

`DeliverOrderTransition`

Domain wrapper around `GovernedTransition<OrderStatus, DeliverOrderRequirement>`. It moves `ShippedWaitingDelivery` to `Delivered` after proof of delivery is captured.

`CancelOrderTransition`

Domain wrapper around `GovernedTransition<OrderStatus, CancelOrderRequirement>`. It moves cancellable states to `Cancelled` and rejects shipped or delivered orders.

## Transitions/Preconditions

`PaymentMustBeCapturedPrecondition`

Requires the payment provider to confirm capture before the order can await fulfillment.

`PaymentReferenceRequiredPrecondition`

Requires captured payment to carry a provider reference.

`CarrierRequiredPrecondition`

Requires shipment to identify a carrier.

`TrackingCodeRequiredPrecondition`

Requires shipment to identify a tracking code.

`ProofOfDeliveryRequiredPrecondition`

Requires delivery to include proof of delivery.

`CancellationReasonRequiredPrecondition`

Requires cancellation to include an auditable reason.

`OrderMustBeCancellablePrecondition`

Rejects cancellation once an order is shipped or delivered.

## Invariants

`OrderFulfillmentInvariants`

Groups executable invariants for fulfillment. `PrepareForShipping` currently protects `PaymentCapturedBeforeShipmentPreparation` and `StockReservedBeforeShipmentPreparation`.

## SideEffects

`FulfillmentNotification`

Small message record used as the context for fulfillment notification side effects.

`NotifyFulfillmentSideEffect`

Domain wrapper around `GovernedSideEffect<FulfillmentNotification>`. It executes once per idempotency key and emits side-effect evidence.

`NotifyFulfillmentLifecycle`

Domain wrapper around `GovernedSideEffectLifecycle<FulfillmentNotification>`. It models planned, persisted, published, confirmed, compensation-required, and compensated lifecycle states. Its `PlanPersistAndPublish` method shows the compact helper path for the common application flow.

## Evidence

`OrderingEvidencePublisher`

Forwards `Order.BusinessEvidence` into an `IBusinessEvidenceSink` through `BusinessEvidenceRecorder`. This keeps the domain model independent from logging and tracing infrastructure.

## Generated Context

`agent-context.md`

Exported `AgentContextPacket` markdown for the sample. It gives humans and agents a stable read-before-change context without executing code.
