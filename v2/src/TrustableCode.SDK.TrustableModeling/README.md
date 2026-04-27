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

## Minimal Example

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

## Main Primitives

`TrustableModelDescriptor`

Describes the model's semantic safety envelope: state model, transitions, invariants, boundaries, side effects, evidence, and non-goals.

`BusinessAdmission<TInput, TAccepted>`

Turns raw external input into admitted business meaning only after named rules pass.

`GovernedTransition<TState, TContext>`

Executes a state transition through named preconditions, invariants, declared events, and declared evidence.

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

## Documentation

See the repository for the current v2 guides and samples:

- `v2/docs/application-service-pattern.md`
- `v2/docs/evidence-conventions.md`
- `v2/docs/testing-helpers.md`
- `v2/samples/TrustableCode.SDK.Samples.Ordering`

## Status

This is a preview package. APIs are still being refined around real sample ergonomics before stable release.
