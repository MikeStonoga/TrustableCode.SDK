# Ordering API Sample

This sample wraps the `Ordering` domain sample in a small ASP.NET Core API with EF Core.

It is intentionally simple, but closer to a real application shape:

- controllers receive HTTP requests
- EF Core stores order snapshots
- EF Core stores outbox messages
- EF Core stores business evidence
- application services still own admission, governed transitions, and side-effect lifecycle

## Run

```bash
dotnet run --project samples/TrustableCode.SDK.Samples.Ordering.Api
```

The API uses EF Core InMemory by default.

If your local ASP.NET Core profile chooses a different port, update `@baseUrl` in `OrderingApi.http`.

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

## Example

The fastest way to exercise the sample is to run the API and execute the requests in `OrderingApi.http`.
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

## Design

The API project depends on the domain sample and adapts it to infrastructure:

- `OrderingDbContext` stores EF entities.
- `EfOrderSnapshotStore` implements `IOrderSnapshotStore`.
- `EfOrderingOutbox` implements `IOrderingOutbox`.
- `EfBusinessEvidenceSink` implements `IBusinessEvidenceSink`.
- `EfOrderingUnitOfWork` commits the EF changes once per HTTP operation.
- `OrdersController` maps HTTP requests to application services.
- `DiagnosticsController` exposes outbox and evidence for sample inspection.

The EF adapters only add changes to the `DbContext`. They do not call `SaveChanges()` directly.
The controller commits after the operation finishes, including rejected requests that still produce business evidence.

The SDK still does not own the web framework, database, or outbox transport. Those remain application concerns.
