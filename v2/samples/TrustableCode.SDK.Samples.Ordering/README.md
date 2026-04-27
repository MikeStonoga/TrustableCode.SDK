# Ordering Sample

This sample starts with the semantic safety envelope for order fulfillment.

Before implementing behavior, a developer or AI agent should inspect:

- `OrderFulfillmentTrustableModel.Descriptor`

The descriptor names the authoritative state, valid transitions, invariants, boundary rules, side effects, evidence, and non-goals that must guide future implementation.

The sample also includes a small executable `Order` model that uses `GovernedTransition` to apply `PrepareForShipping` through named preconditions and declared outputs.

