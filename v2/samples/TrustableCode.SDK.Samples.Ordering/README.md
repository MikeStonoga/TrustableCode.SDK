# Ordering Sample

This sample starts with the semantic safety envelope for order fulfillment.

Before implementing behavior, a developer or AI agent should inspect:

- `OrderFulfillmentTrustableModel.Descriptor`
- `agent-context.md`
- `class-reference.md`

The descriptor names the authoritative state, valid transitions, invariants, boundary rules, side effects, evidence, and non-goals that must guide future implementation.

`agent-context.md` is the exported `AgentContextPacket` for this sample. It lets reviewers and agents inspect the semantic context without running code.

`class-reference.md` explains the role of each sample class and the intended SDK usage path.

The sample also includes an executable `Order` model with the full happy path:

- create through `OrderFactory`
- orchestrate application use through `OrderingApplicationService`
- wait for payment as `PlacedAwaitingPayment`
- capture payment into `PaidAwaitingFulfillment`
- prepare for shipping into `FulfilledReadyForShipping`
- ship into `ShippedWaitingDelivery`
- deliver into `Delivered`
- cancel before shipment through a governed cancellation transition

Each business movement is represented by a specialized transition class:

- `CapturePaymentTransition`
- `PrepareOrderForShippingTransition`
- `ShipOrderTransition`
- `DeliverOrderTransition`
- `CancelOrderTransition`

Each specialized class receives the aggregate through `new SomeTransition(this)` and uses `GovernedTransition` internally.

Transition preconditions are also specialized domain classes under `Transitions/Preconditions/`. They inherit the SDK precondition base, which is also a business invariant rule, so a precondition can be evaluated and evidenced with the same semantic shape as other business truths.

Transition requirements live under `Requirements/` so command meaning stays grouped and easy to scan.

External request DTOs live under `ExternalRequests/`, while admission boundaries live under `Admissions/`. The distinction is intentional: external input is raw, and requirements are admitted business meaning.

`OrderFactory` shows creation as admitted business intent: external callers can ask to create an order, but cannot inject an arbitrary initial status.

`Order.Rehydrate(OrderPersistenceSnapshot)` exists for loading an already-known persisted state; new business creation should go through the factory.

Admission classes turn external input into admitted business intent only after boundary rules pass:

- `CapturePaymentAdmission`
- `PrepareOrderForShippingAdmission`
- `ShipOrderAdmission`
- `DeliverOrderAdmission`
- `CancelOrderAdmission`

`NotifyFulfillmentSideEffect` shows the first governed side-effect shape: execute once per idempotency key and emit structured evidence for executed or already-applied attempts.

`NotifyFulfillmentLifecycle` shows the durable lifecycle around the same business effect: planned, persisted, published, confirmed, compensation required, and compensated.

`OrderingEvidencePublisher` shows how business evidence can be forwarded to a sink without coupling the domain model to logging or tracing infrastructure.

`PersistedOrderingApplicationService` shows the same application flow around persisted snapshots and an in-memory outbox: load, rehydrate, execute, save, enqueue produced events, and publish evidence.

`ActivitySourceBusinessEvidenceSink` can then turn the same evidence into trace activities with business-oriented tags.

`LoggerBusinessEvidenceSink` emits the same evidence through `ILogger` with stable structured fields.
