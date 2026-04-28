# Ordering Sample

This sample shows how to use `TrustableCode.SDK.TrustableModeling` in an order fulfillment flow.

Start here when you want to see the SDK used in application code, not only as isolated primitives.

## Read First

- `agent-context.md` is the exported semantic context for reviewers and AI agents.
- `class-reference.md` explains every class in the sample.
- `OrderFulfillmentTrustableModel.Descriptor` declares states, transitions, invariants, boundaries, side effects, evidence, and non-goals.

## I Want To Create An Order

Use `OrderFactory.Create(...)`.

Creation is a boundary, not a direct constructor call. External callers submit `ExternalCreateOrderRequest`; the factory admits or rejects that input before creating the aggregate.

```csharp
var result = OrderFactory.Create(new ExternalCreateOrderRequest(
    OrderId: "order-1",
    CustomerId: "customer-1",
    Lines: [new OrderLine("sku-1", 1)],
    RequestedStatus: null,
    CorrelationId: "corr-create-1"));

var order = result.Value;
```

The request intentionally has `RequestedStatus`. The admission boundary rejects it when callers try to inject arbitrary initial state.

## I Want To Run A Business Operation In Memory

Use `OrderingApplicationService`.

It composes the application-level flow:

```text
ExternalRequest -> Admission -> Requirement -> Order -> GovernedTransition -> Evidence -> SideEffectLifecycle
```

Example:

```csharp
var service = new OrderingApplicationService(evidenceSink, sideEffectLifecycleStore);

var result = service.PrepareForShipping(order, new ExternalPrepareOrderForShippingRequest(
    PaymentCaptured: true,
    StockReserved: true,
    RequestedStatus: "",
    CorrelationId: "corr-prepare-1"));
```

This is the clearest entry point for understanding how admissions, transitions, evidence, and side-effect lifecycle work together.

## I Want To Load From Persistence And Save Changes

Use `PersistedOrderingApplicationService`.

It demonstrates the real application shape around persistence:

```text
Snapshot -> Rehydrate -> Application Service -> Save Snapshot -> Outbox -> Evidence
```

Example:

```csharp
orders.Save(new OrderPersistenceSnapshot(
    OrderId: "order-1",
    CustomerId: "customer-1",
    Lines: [new OrderLine("sku-1", 1)],
    Status: OrderStatus.PaidAwaitingFulfillment));

var service = new PersistedOrderingApplicationService(
    orders,
    outbox,
    evidenceSink,
    sideEffectLifecycleStore);

var result = service.PrepareForShipping(
    "order-1",
    new ExternalPrepareOrderForShippingRequest(
        PaymentCaptured: true,
        StockReserved: true,
        RequestedStatus: "",
        CorrelationId: "corr-prepare-1"));
```

Rejected operations may publish rejection evidence, but they do not save a new snapshot or enqueue success events.

## I Want To Understand State Changes

`Order.Status` is read-only to callers.

Internally, `Order` owns a `GovernedState<OrderStatus>` named `StatusState`. Transition classes use:

```csharp
.State(order.StatusState)
```

The SDK calls `ApplyApproved` only after the transition passes state checks, preconditions, and invariants.

## I Want To Understand Boundaries

External DTOs live under `ExternalRequests/`.

Admission rules live under `Admissions/`.

Accepted business intent is represented by records under `Requirements/`.

This separation is intentional:

- external requests are raw input
- admissions decide whether input has business meaning
- requirements are admitted intent passed into transitions

## I Want To Understand Evidence And Outbox

`OrderingEvidencePublisher` forwards `Order.BusinessEvidence` to an `IBusinessEvidenceSink`.

`PersistedOrderingApplicationService` copies successful transition events into `IOrderingOutbox`.

`NotifyFulfillmentLifecycle` demonstrates planned, persisted, and published lifecycle evidence for external side effects.

## Folder Map

- `ExternalRequests/`: raw input shapes.
- `Admissions/`: boundary rules that accept or reject input.
- `Requirements/`: admitted business intent.
- `Transitions/`: domain-specific wrappers around `GovernedTransition`.
- `Transitions/Preconditions/`: reusable transition rules.
- `Invariants/`: executable business invariants.
- `Application/`: application service examples.
- `Persistence/`: snapshot and in-memory repository example.
- `Outbox/`: in-memory outbox example.
- `SideEffects/`: governed side-effect examples.
- `Evidence/`: evidence publishing adapter.

## Related Docs

- `../../docs/application-service-pattern.md`
- `../../docs/persisted-application-flow.md`
- `../../docs/evidence-conventions.md`
- `../../docs/testing-helpers.md`
