# Ordering API Sample

This sample wraps the `Ordering` domain sample in a small ASP.NET Core API with EF Core.
For the reusable integration pattern behind this sample, see `../../docs/aspnetcore-api-integration.md`.

It is intentionally simple, but closer to a real application shape:

- controllers receive HTTP requests
- EF Core stores order snapshots
- EF Core stores outbox messages
- EF Core stores business evidence
- EF Core stores side-effect lifecycle records
- application services still own admission, governed transitions, and side-effect lifecycle

## Run

```bash
dotnet run --project samples/TrustableCode.SDK.Samples.Ordering.Api
```

The API uses SQLite by default and creates `ordering-sample.db` in the current working directory.
The schema is created automatically on startup with `EnsureCreated()` so the sample can run without EF tooling.

To run without writing a database file, switch the provider to InMemory:

```bash
dotnet run --project samples/TrustableCode.SDK.Samples.Ordering.Api --OrderingDatabase:Provider=InMemory
```

To choose another SQLite file:

```bash
dotnet run --project samples/TrustableCode.SDK.Samples.Ordering.Api --OrderingDatabase:ConnectionString="Data Source=data/ordering-sample.db"
```

If your local ASP.NET Core profile chooses a different port, update `@baseUrl` in `OrderingApi.http`.

Open Swagger UI after the API starts:

```text
http://localhost:5000/swagger
```

The root URL redirects to Swagger UI for convenience.

## Endpoints

Create and inspect orders:

- `POST /api/orders`
- `GET /api/orders/{orderId}`

Run business operations:

- `POST /api/orders/{orderId}/capture-payment`
- `POST /api/orders/{orderId}/prepare-for-shipping`
- `POST /api/orders/{orderId}/ship`
- `POST /api/orders/{orderId}/deliver`
- `POST /api/orders/{orderId}/cancel`

Inspect application output:

- `GET /api/diagnostics/outbox`
- `GET /api/diagnostics/evidence`
- `GET /api/diagnostics/side-effect-lifecycles`

## Example

The fastest way to exercise the sample is to run the API and execute the requests in `OrderingApi.http`.
Use Swagger UI when you want to browse endpoints and response contracts in the browser.
It includes the full happy path:

```text
create -> capture-payment -> prepare-for-shipping -> ship -> deliver
```

It also includes rejection examples showing that external callers cannot directly force a status.

Create request shape:

```json
{
  "orderId": "order-1",
  "customerId": "customer-1",
  "lines": [
    { "sku": "sku-1", "quantity": 1 }
  ],
  "requestedStatus": null,
  "correlationId": "corr-create-1"
}
```

Prepare-for-shipping request shape:

```json
{
  "paymentCaptured": true,
  "stockReserved": true,
  "requestedStatus": "",
  "correlationId": "corr-prepare-1"
}
```

After running the flow, inspect:

- `GET /api/orders/{orderId}` for the persisted snapshot.
- `GET /api/diagnostics/outbox` for events produced by approved operations.
- `GET /api/diagnostics/evidence` for business evidence produced by admissions and transitions.
- `GET /api/diagnostics/side-effect-lifecycles` for durable side-effect lifecycle records.

Operation responses include a developer-facing summary:

- `outcome`: `created`, `applied`, `alreadyApplied`, `admissionRejected`, or `transitionRejected`.
- `message`: a short explanation of what happened.
- `failureStage`: `admission` for boundary rejections, `transition` for governed transition conflicts, or `null` on success.
- `currentStatus`: the aggregate status after the operation, when an order was available.
- `rejectionReasons`: the raw business reasons from the admission or transition.
- `sideEffectLifecycle`: the planned/persisted/published side effect record when an operation emits one.

## Design

The API project depends on the domain sample and adapts it to infrastructure:

- `OrderingDbContext` stores EF entities.
- SQLite is the default runtime provider; EF InMemory is available for fast local resets.
- `EfOrderSnapshotStore` implements `IOrderSnapshotStore`.
- `EfOrderingOutbox` implements `IOrderingOutbox`.
- `EfBusinessEvidenceSink` implements `IBusinessEvidenceSink`.
- `EfSideEffectLifecycleStore` implements `ISideEffectLifecycleStore`.
- `EfOrderingUnitOfWork` commits the EF changes once per HTTP operation.
- `OrdersController` maps HTTP requests to application services.
- `DiagnosticsController` exposes outbox and evidence for sample inspection.
- Swagger/OpenAPI exposes the sample endpoints and response contracts.

The EF adapters only add changes to the `DbContext`. They do not call `SaveChanges()` directly.
The controller commits after the operation finishes, including rejected requests that still produce business evidence.

The API configures JSON enum serialization as strings so order and transition statuses are readable in HTTP clients.

The SDK still does not own the web framework, database, or outbox transport. Those remain application concerns.
