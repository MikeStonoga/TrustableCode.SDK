# Testing Helpers

`TrustableChecks` provides framework-neutral checks for the SDK outputs.

The helpers do not depend on xUnit, NUnit, MSTest, or FluentAssertions. They return a `TrustableCheck` with:

- `Passed`
- `Failures`
- `ThrowIfFailed()`

## Transition Applied

```csharp
var result = order.PrepareForShipping(requirement);

TrustableChecks.TransitionApplied(
    result,
    OrderStatus.FulfilledReadyForShipping,
    producedEvents: ["OrderPreparedForShipping"],
    producedEvidence: ["OrderPreparedForShippingEvidence"])
    .ThrowIfFailed();
```

## Transition Rejected

```csharp
var result = order.Cancel(requirement);

TrustableChecks.TransitionRejected(
    result,
    rejectionReasons: ["Orders waiting for delivery or already delivered cannot be cancelled by this transition."])
    .ThrowIfFailed();
```

## Admission

```csharp
var result = ShipOrderAdmission.Create().Admit(request);

TrustableChecks.AdmissionRejected(
    result,
    rejectionEvidence: ["OrderShipmentRejectedEvidence"])
    .ThrowIfFailed();
```

## Invariant

```csharp
var evaluation = invariant.Evaluate(context);

TrustableChecks.InvariantViolated(
    evaluation,
    expectedCode: "StockReservedBeforeShipmentPreparation")
    .ThrowIfFailed();
```

## Side Effect And Evidence

```csharp
var result = sideEffect.Execute(notification);

TrustableChecks.SideEffectStatus(
    result,
    SideEffectExecutionStatus.Executed,
    expectedEvidenceName: "NotifyFulfillmentEvidence")
    .ThrowIfFailed();
```

Use these helpers when the test should read like business expectations instead of plumbing assertions.
