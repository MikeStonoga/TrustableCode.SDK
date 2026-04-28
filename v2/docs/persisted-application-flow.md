# Persisted Application Flow

This guide documents the recommended shape for using v2 with persisted aggregate state.

It is intentionally a pattern guide, not a new SDK abstraction. Repositories, transactions, outboxes, and database mappings are application concerns. The SDK should help preserve business meaning inside those flows without owning the storage technology.

## Flow

Use this order when a command targets an aggregate that already exists in storage:

1. Load a persistence snapshot from the application repository.
2. Rehydrate the aggregate from trusted persisted state.
3. Run the application service flow: admission, requirement, governed transition, evidence.
4. Save a new persistence snapshot only when the operation succeeds.
5. Enqueue produced events into an outbox only when the operation succeeds.
6. Publish or record business evidence for accepted and rejected behavior.

In short:

```text
Snapshot -> Rehydrate -> Admission -> GovernedTransition -> Save Snapshot -> Outbox -> Evidence
```

## Snapshot

Snapshots should represent trusted persisted state, not external creation input.

```csharp
public sealed record OrderPersistenceSnapshot(
    string OrderId,
    string CustomerId,
    IReadOnlyCollection<OrderLine> Lines,
    OrderStatus Status);
```

Rehydration should not emit creation evidence:

```csharp
var order = Order.Rehydrate(snapshot);
```

Creation evidence belongs to the creation boundary. Rehydration is loading already-known state.

## Repository

The repository should store snapshots, not live aggregate instances:

```csharp
public interface IOrderSnapshotStore
{
    OrderPersistenceSnapshot? Find(string orderId);

    void Save(OrderPersistenceSnapshot snapshot);
}
```

This keeps persistence explicit and makes the boundary between storage and business behavior easy to inspect.

## Outbox

Produced events from `TransitionExecutionResult` can be copied into an application outbox after a successful operation:

```csharp
foreach (var eventName in result.ProducedEvents)
{
    outbox.Enqueue(new OrderingOutboxMessage(
        StreamName: $"Order-{order.OrderId}",
        EventName: eventName,
        OrderId: order.OrderId,
        CorrelationId: correlationId));
}
```

The SDK does not need to own the outbox transport. It only needs to preserve the semantic event names and evidence that explain why work was accepted or rejected.

## Application Service Shape

The persisted application service should stay explicit:

```csharp
var snapshot = orders.Find(orderId)
    ?? throw new InvalidOperationException($"Order '{orderId}' was not found in persistence.");

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

The key rule is simple: rejected operations may publish rejection evidence, but they should not save a new aggregate snapshot or enqueue success events.

## What Belongs Where

Keep these in the application:

- repository interfaces and database implementations
- transaction boundaries
- outbox storage and dispatching
- serialization and schema mapping
- aggregate snapshot shape

Keep these in the SDK:

- admission rules
- governed transitions
- invariant evaluation
- side-effect lifecycle
- business evidence
- test checks
- agent context generation

Promote a persistence abstraction into the SDK only after multiple samples need the same shape.

## Ordering Sample

See:

- `samples/TrustableCode.SDK.Samples.Ordering/Application/PersistedOrderingApplicationService.cs`
- `samples/TrustableCode.SDK.Samples.Ordering/Persistence`
- `samples/TrustableCode.SDK.Samples.Ordering/Outbox`

The sample uses in-memory implementations only to explain the pattern. Production applications should replace them with real storage and transactional outbox infrastructure.
