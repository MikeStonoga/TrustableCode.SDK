# Application Service Pattern

This is the recommended application-layer pattern for using the v2 modeling primitives.

The goal is to keep business meaning explicit without forcing every application service to repeat the same plumbing.

## Flow

Use this order when external input wants to change business state:

1. Receive an external request DTO.
2. Admit or reject it with `BusinessAdmission`.
3. Convert admitted input into a requirement record.
4. Execute a specialized domain transition backed by `GovernedTransition`.
5. Publish `BusinessEvidence` from accepted or rejected behavior.
6. Advance side-effect lifecycle when external work must be durable or idempotent.

In short:

```text
ExternalRequest -> BusinessAdmission -> Requirement -> GovernedTransition -> BusinessEvidence -> SideEffectLifecycle
```

## Admission Then Transition

Declare admission boundaries with the fluent admission builder when that reads better than manually constructing rules:

```csharp
var admission = BusinessAdmission<ExternalShipOrderRequest, ShipOrderRequirement>
    .Create("ShipOrderAdmission")
    .RejectWhen(
        code: "BoundaryMustReceiveShipmentIntentNotStatus",
        description: "The boundary accepts shipment facts, not direct status mutation.",
        shouldReject: request => !string.IsNullOrWhiteSpace(request.RequestedStatus),
        rejectionReason: "External callers may confirm shipment, but may not submit an arbitrary target order status.",
        rejectionEvidenceName: "OrderShipmentRejectedEvidence")
    .AcceptWith(request => new ShipOrderRequirement(
        request.Carrier,
        request.TrackingCode,
        request.CorrelationId))
    .Build();
```

Use `TrustableAdmissionFlow.ExecuteTransition` when the application service should stop before domain behavior if admission rejects the input.

```csharp
var result = TrustableAdmissionFlow.ExecuteTransition(
    ShipOrderAdmission.Create(),
    request,
    order.Ship);

if (!result.WasAccepted)
{
    evidenceRecorder.RecordMany(result.RejectionEvidence);
    return OrderingApplicationResult.Rejected("ShipOrder", result.RejectionReasons);
}

orderingEvidencePublisher.Publish(order);

return OrderingApplicationResult.FromTransition(
    "ShipOrder",
    order,
    result.Transition!);
```

This keeps the policy visible:

- admission owns external input validation
- the aggregate owns state movement
- evidence is published by the application layer
- rejected input does not call the transition

## Governed Transition Declaration

Declare transitions with the fluent builder when creating domain-specific transition classes:

```csharp
var transition = GovernedTransition<OrderStatus, ShipOrderRequirement>
    .Create("ShipOrder")
    .From(OrderStatus.FulfilledReadyForShipping)
    .To(OrderStatus.ShippedWaitingDelivery)
    .ReadState(() => order.Status)
    .ApplyState(order.ApplyStatus)
    .Require(new CarrierRequiredPrecondition())
    .Require(new TrackingCodeRequiredPrecondition())
    .ProducesEvent("OrderShipped")
    .ProducesEvidence("OrderShippedEvidence")
    .TreatRepetitionAsAlreadyApplied()
    .Build();
```

`ReadState` is how the SDK observes the aggregate state. `ApplyState` is the callback it invokes only after the transition is approved.

## Side-Effect Lifecycle

Use `PlanPersistAndPublish` for the common application flow where an external side effect must be planned, persisted, and published in order.

```csharp
var notification = new FulfillmentNotification(order.OrderId, correlationId);

var published = fulfillmentLifecycle.PlanPersistAndPublish(
    notification,
    evidenceRecorder);
```

The helper records evidence for each lifecycle step:

- planned
- persisted
- published

Keep explicit lifecycle calls when the application needs a different policy, such as confirmation, compensation, retries, or worker-driven publishing.

## Recommended Shape

An application service should be boring and explicit:

```csharp
public OrderingApplicationResult Ship(Order order, ExternalShipOrderRequest request)
{
    var result = TrustableAdmissionFlow.ExecuteTransition(
        ShipOrderAdmission.Create(),
        request,
        order.Ship);

    return Complete("ShipOrder", order, result);
}
```

The service should not hide business decisions behind generic handlers too early. Prefer a small method per business operation until the repetition is proven and the semantic names remain clear.

## Ordering Sample

See:

- `samples/TrustableCode.SDK.Samples.Ordering/Application/OrderingApplicationService.cs`
- `samples/TrustableCode.SDK.Samples.Ordering/class-reference.md`
- `samples/TrustableCode.SDK.Samples.Ordering/agent-context.md`

The sample shows creation, payment capture, shipment preparation, shipping, delivery, cancellation, evidence publishing, and side-effect lifecycle in one place.
