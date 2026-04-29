# ASP.NET Core API Integration

This guide shows the recommended shape for using `TrustableCode.SDK.TrustableModeling` from an ASP.NET Core API.

The SDK should stay in the application/domain flow. ASP.NET Core, EF Core, outbox transport, and HTTP response formatting should stay in your application.

## Recommended Shape

Use this dependency direction:

```text
Controller -> Query Service for reads
Controller -> Persisted Application Service -> Application Service -> Trustable SDK primitives
Persisted Application Service -> Unit Of Work commit
Infrastructure adapters -> EF Core / database / outbox tables
```

Keep these responsibilities separate:

- Controllers translate HTTP into external request records, call application services, and choose HTTP status codes.
- Query services read persisted snapshots for HTTP read models.
- Application services compose admissions, requirements, governed transitions, evidence, and side-effect lifecycle.
- Persisted application services load snapshots, rehydrate aggregates, save successful snapshots, and enqueue successful outbox events.
- EF adapters implement persistence ports such as snapshot store, outbox, evidence sink, and lifecycle store.
- Unit of Work is invoked by the persisted application service once per application operation.
- Diagnostics endpoints expose sample state, but production systems should usually use observability and admin tooling instead.

## Service Registration

The Ordering API sample keeps the registration in extension methods so `Program.cs` remains small:

```csharp
builder.Services.AddOrderingApiControllers();
builder.Services.AddOrderingApiSwagger();
builder.Services.AddOrderingInfrastructure(builder.Configuration);
```

The infrastructure extension registers EF Core and the application adapters:

```csharp
services.AddDbContext<OrderingDbContext>(options =>
{
    var provider = configuration["OrderingDatabase:Provider"] ?? "Sqlite";
    if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("ordering-sample");
        return;
    }

    var connectionString = configuration.GetConnectionString("OrderingDatabase")
        ?? configuration["OrderingDatabase:ConnectionString"]
        ?? "Data Source=ordering-sample.db";

    options.UseSqlite(connectionString);
});

services.AddScoped<IOrderSnapshotStore, EfOrderSnapshotStore>();
services.AddScoped<IOrderingOutbox, EfOrderingOutbox>();
services.AddScoped<IBusinessEvidenceSink, EfBusinessEvidenceSink>();
services.AddScoped<ISideEffectLifecycleStore, EfSideEffectLifecycleStore>();
services.AddScoped<IOrderingUnitOfWork, EfOrderingUnitOfWork>();
services.AddScoped<OrderingApplicationService>();
services.AddScoped<PersistedOrderingApplicationService>();
services.AddScoped<OrderingQueryService>();
```

The important part is that the SDK interfaces are registered through application-owned adapters. The SDK does not know about ASP.NET Core or EF Core.

## Unit Of Work

Adapters should add changes to the current persistence context. They should not decide when the operation is complete.

```csharp
public sealed class EfOrderingOutbox(OrderingDbContext db) : IOrderingOutbox
{
    public void Enqueue(OrderingOutboxMessage message)
    {
        db.OutboxMessages.Add(new OrderingOutboxMessageEntity
        {
            StreamName = message.StreamName,
            EventName = message.EventName,
            OrderId = message.OrderId,
            CorrelationId = message.CorrelationId
        });
    }
}
```

Commit once at the application operation boundary:

```csharp
public sealed class EfOrderingUnitOfWork(OrderingDbContext db) : IOrderingUnitOfWork
{
    public void Commit()
        => db.SaveChanges();
}
```

Commit even when admission rejects input if your application recorded business evidence for that rejection.

## Controller Pattern

Controllers should not mutate aggregate state directly. They should call the application service and map the result to HTTP.

```csharp
var result = persistedApplication.CreateOrder(request);

if (!result.Succeeded)
{
    return BadRequest(OperationResponse.From(result));
}

return CreatedAtAction(nameof(Get), new { orderId = result.Order!.OrderId }, OperationResponse.From(result));
```

Snapshot persistence and outbox enqueueing belong inside the persisted application service, not in the controller.
The controller owns HTTP translation only. Transaction commit belongs to the persisted application service.
For reads, controllers should call query services instead of reaching directly into repositories.

Use different HTTP outcomes for different trustable outcomes:

- Admission rejection: `400 Bad Request`.
- Accepted operation rejected by governed transition: `409 Conflict`.
- Missing persisted aggregate: `404 Not Found`.
- Applied or already-applied operation: `200 OK` or `201 Created`.

## Persistence Ports

For persisted aggregate flows, define application ports instead of letting the SDK own storage:

- `IOrderSnapshotStore`: load/save trusted aggregate snapshots.
- `IOrderingOutbox`: enqueue events produced by successful operations.
- `IBusinessEvidenceSink`: persist structured evidence.
- `ISideEffectLifecycleStore`: persist durable side-effect lifecycle records.

This keeps the SDK portable and lets each application choose EF Core, Dapper, event sourcing, a message broker, or another persistence strategy.

## Runtime Provider

The sample API uses SQLite by default:

```json
{
  "OrderingDatabase": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=ordering-sample.db"
  }
}
```

For quick resets, run with InMemory:

```bash
dotnet run --project samples/TrustableCode.SDK.Samples.Ordering.Api --OrderingDatabase:Provider=InMemory
```

For production-style systems, prefer real migrations over `EnsureCreated()`. The sample uses `EnsureCreated()` to stay self-contained.

## Testing

Use direct adapter tests for persistence mapping and HTTP integration tests for the real API contract.

The sample uses:

- SQLite in-memory for relational persistence tests.
- `WebApplicationFactory` for endpoint tests.
- JSON assertions against actual HTTP responses, not controller-only objects.

This catches routing, serialization, dependency injection, status codes, and persistence behavior together.

## Checklist

- External input is represented by explicit request records.
- Admissions reject invalid boundary input before domain state changes.
- Transitions own state changes through governed rules.
- Evidence is recorded for accepted, rejected, and side-effect operations.
- Outbox messages are produced only after successful operations.
- Unit of Work commits once per application operation.
- Tests cover success, admission rejection, transition rejection, persistence, and HTTP JSON.
