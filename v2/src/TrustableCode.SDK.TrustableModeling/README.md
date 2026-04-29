# TrustableCode.SDK.TrustableModeling

`TrustableCode.SDK.TrustableModeling` provides semantic modeling primitives for codebases where business behavior must stay understandable and safe to change.

The package is focused on explicit meaning:

- authoritative state
- governed transitions
- protected invariants
- admitted external input
- controlled side effects
- observable business evidence
- context packets for humans and AI agents

## Recommended Flow

For application code that receives external input and changes business state, use this shape:

```text
ExternalRequest -> BusinessAdmission -> Requirement -> GovernedTransition -> BusinessEvidence -> SideEffectLifecycle
```

The SDK keeps each responsibility visible:

- `BusinessAdmission` accepts or rejects external input before it becomes business meaning.
- Requirement records carry admitted intent into the domain.
- `GovernedTransition` applies state changes only after state, preconditions, and invariants pass.
- `BusinessEvidence` records accepted, rejected, transition, invariant, and side-effect behavior.
- `GovernedSideEffectLifecycle` tracks external work that must be idempotent, durable, or compensatable.
- `AgentContextPacket` exports model context for reviewers and AI coding agents.

## Minimal Application Service Example

```csharp
var result = TrustableAdmissionFlow.ExecuteTransition(
    ShipOrderAdmission.Create(),
    request,
    order.Ship);

if (!result.WasAccepted)
{
    evidenceRecorder.RecordMany(result.RejectionEvidence);
    return;
}

orderingEvidencePublisher.Publish(order);
```

For side effects that must be planned, persisted, and published:

```csharp
var notification = new FulfillmentNotification(order.OrderId, correlationId);

var published = fulfillmentLifecycle.PlanPersistAndPublish(
    notification,
    evidenceRecorder);
```

Keep explicit lifecycle calls when your application needs confirmation, compensation, retries, or worker-driven publishing.

## Persisted Application Flow

The SDK does not own repositories, transactions, or outbox transport. Keep those as application concerns and let the SDK preserve business meaning inside that flow:

```text
Snapshot -> Rehydrate -> Admission -> GovernedTransition -> Save Snapshot -> Outbox -> Evidence
```

Typical application code:

```csharp
var snapshot = orders.Find(orderId)
    ?? throw new InvalidOperationException($"Order '{orderId}' was not found.");

var order = Order.Rehydrate(snapshot);
var result = application.PrepareForShipping(order, request);

if (!result.Succeeded)
{
    return result;
}

orders.Save(OrderPersistenceSnapshot.From(order));

foreach (var eventName in result.ProducedEvents)
{
    outbox.Enqueue(new OrderingOutboxMessage(
        StreamName: $"Order-{order.OrderId}",
        EventName: eventName,
        OrderId: order.OrderId,
        CorrelationId: request.CorrelationId));
}

return result;
```

Rejected operations may record rejection evidence, but they should not save a new aggregate snapshot or enqueue success events.

## ASP.NET Core Integration

In web APIs, keep HTTP and persistence at the edges:

```text
Controller -> Persisted Application Service -> Application Service -> Trustable SDK primitives
Persisted Application Service -> Unit Of Work commit
Infrastructure adapters -> EF Core / database / outbox tables
```

Recommended HTTP mapping:

- admission rejected: `400 Bad Request`
- accepted operation rejected by governed transition: `409 Conflict`
- missing persisted aggregate: `404 Not Found`
- applied or already-applied operation: `200 OK` or `201 Created`

Snapshot persistence, outbox enqueueing, and Unit of Work commit should live in the application layer, not in controllers.
Adapters should add changes to the current persistence context. A Unit of Work should commit once per application operation, including rejected admissions when rejection evidence was recorded.

The repository sample includes an ASP.NET Core API with controllers, EF Core SQLite, outbox storage, evidence storage, side-effect lifecycle persistence, Swagger, `.http` examples, and `WebApplicationFactory` tests.

## Main Primitives

`TrustableModelDescriptor`

Describes the model's semantic safety envelope: state model, transitions, invariants, boundaries, side effects, evidence, and non-goals.

`BusinessAdmission<TInput, TAccepted>`

Turns raw external input into admitted business meaning only after named rules pass.

Use `BusinessAdmission<TInput, TAccepted>.Create(...)` when declaring boundaries fluently:

```csharp
var admission = BusinessAdmission<ExternalShipOrderRequest, ShipOrderRequirement>
    .Create("ShipOrderAdmission")
    .Require(
        code: "CorrelationIdRequired",
        description: "A correlation id is required so shipment can be traced.",
        isSatisfied: request => !string.IsNullOrWhiteSpace(request.CorrelationId),
        rejectionReason: "A correlation id is required before shipment can be admitted.",
        rejectionEvidenceName: "OrderShipmentRejectedEvidence")
    .AcceptWith(request => new ShipOrderRequirement(
        request.Carrier,
        request.TrackingCode,
        request.CorrelationId))
    .Build();
```

`GovernedTransition<TState, TContext>`

Executes a state transition through named preconditions, invariants, declared events, and declared evidence.

`GovernedState<TState>` can hold state that should only be changed by approved transitions:

```csharp
internal GovernedState<OrderStatus> StatusState { get; }

public OrderStatus Status => StatusState.Current;
```

Use `GovernedTransition<TState, TContext>.Create(...)` to declare transitions fluently:

```csharp
var transition = GovernedTransition<OrderStatus, ShipOrderRequirement>
    .Create("ShipOrder")
    .From(OrderStatus.FulfilledReadyForShipping)
    .To(OrderStatus.ShippedWaitingDelivery)
    .State(order.StatusState)
    .Require(new CarrierRequiredPrecondition())
    .ProducesEvent("OrderShipped")
    .ProducesEvidence("OrderShippedEvidence")
    .TreatRepetitionAsAlreadyApplied()
    .Build();
```

`State(...)` tells the SDK how to inspect and apply state. The transition calls `ApplyApproved` only after state, preconditions, and invariants pass.

`BusinessInvariant<TContext>`

Represents an executable business truth with stable code, severity, descriptor, and violation evidence.

`GovernedSideEffect<TContext>`

Runs an external effect once per idempotency key and emits side-effect evidence.

`GovernedSideEffectLifecycle<TContext>`

Tracks planned, persisted, published, confirmed, compensation-required, and compensated side-effect states.

`BusinessEvidence`

The structured observable record for business-relevant transitions, rejections, invariant violations, side effects, reconciliation, compensation, and audit behavior.

`AgentContextPacket`

Exports a markdown context packet so humans and AI agents can inspect business meaning before changing code.

## Testing

The package includes framework-neutral test helpers in `TrustableCode.SDK.TrustableModeling.Testing`.

Use tests to assert semantic behavior directly:

- admissions accept or reject the right external input
- transitions produce the expected status, events, evidence, and rejection reasons
- invariants protect state changes
- side effects preserve idempotency
- API integration tests validate actual HTTP status codes and JSON response contracts

## Documentation

See the repository for the current v2 guides and samples:

- `v2/docs/application-service-pattern.md`
- `v2/docs/persisted-application-flow.md`
- `v2/docs/aspnetcore-api-integration.md`
- `v2/docs/evidence-conventions.md`
- `v2/docs/testing-helpers.md`
- `v2/samples/TrustableCode.SDK.Samples.Ordering`
- `v2/samples/TrustableCode.SDK.Samples.Ordering.Api`

## Status

This is a preview package. APIs are still being refined around real sample ergonomics before stable release.
