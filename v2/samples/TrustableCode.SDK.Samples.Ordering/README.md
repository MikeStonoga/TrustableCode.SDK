# Ordering Sample

This sample starts with the semantic safety envelope for order fulfillment.

Before implementing behavior, a developer or AI agent should inspect:

- `OrderFulfillmentTrustableModel.Descriptor`

The descriptor names the authoritative state, valid transitions, invariants, boundary rules, side effects, evidence, and non-goals that must guide future implementation.

The sample also includes a small executable `Order` model. The aggregate delegates to a specialized `PrepareOrderForShippingTransition`, which receives the aggregate through `new PrepareOrderForShippingTransition(this)` and uses `GovernedTransition` internally.

The boundary sample uses `PrepareOrderForShippingAdmission` to turn external input into admitted business intent only after boundary rules pass.

`NotifyFulfillmentSideEffect` shows the first governed side-effect shape: execute once per idempotency key and emit structured evidence for executed or already-applied attempts.

`NotifyFulfillmentLifecycle` shows the durable lifecycle around the same business effect: planned, persisted, published, confirmed, compensation required, and compensated.

`OrderingEvidencePublisher` shows how business evidence can be forwarded to a sink without coupling the domain model to logging or tracing infrastructure.

`ActivitySourceBusinessEvidenceSink` can then turn the same evidence into trace activities with business-oriented tags.

`LoggerBusinessEvidenceSink` emits the same evidence through `ILogger` with stable structured fields.
