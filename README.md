# TrustableCode.SDK

`TrustableCode.SDK` is a `.NET` library suite for making business meaning more explicit in software that cannot afford careless change.

It applies the core discipline from *Trustable Code* in reusable primitives that help teams:

- protect important state
- constrain dangerous transitions
- preserve business invariants
- model business intent explicitly
- record meaningful business events
- emit business observability evidence instead of infrastructure noise alone

## Packages

### `TrustableCode.SDK.BusinessModeling`

Core primitives for explicit business modeling:

- `AggregateRoot`
- `BusinessEntity`
- `ValueObject`
- `TypedId<TValue>`
- `BusinessIntent<TPayload>`
- `IBusinessInvariantRule`
- `BusinessNotification`
- `BusinessInvariantManifest<TInvariant>`
- `BusinessTransition<TState>`
- `NamedBusinessTransition<TState>`
- `BusinessEvent`
- `BusinessEventOutboxMessage`
- `IBusinessEventOutbox`
- `IBusinessEventPublisher`
- `IBusinessTransactionRunner`

### `TrustableCode.SDK.BusinessModeling.Observability`

Observability primitives for business evidence:

- `BusinessTransitionEvidence<TState>`
- `InvariantViolationEvidence`
- `SideEffectEvidence`
- `IBusinessEvidence`
- `IBusinessEvidenceSink`
- `BusinessEvidenceLoggerSink`
- `BusinessActivitySourceSink`

## Why this exists

Most libraries help with plumbing.

This SDK is intended to help with meaning.

It is designed to make code more explicit about:

- what state matters
- which truths must remain preserved
- which transitions are legitimate
- which business events deserve to be recorded
- how to make domain behavior observable after deployment

## Example

The repository includes a full sample at:

- `samples/TrustableCode.SDK.BusinessModeling.Example.Ordering`

The sample shows:

- strongly typed identifiers
- explicit business intent
- invariant manifests
- specialized business exceptions
- named transitions
- business events
- transactional outbox persistence before publication
- specialized business transition evidence
- an application service that persists behavior and emits evidence

## Current status

The SDK currently provides a strong foundation for:

- explicit business modeling
- reusable invariant protection
- safer transitions
- domain-level observability

Next layers can extend this into:

- richer examples
- additional integration adapters
- packaging and publishing workflows
- more domain-specific patterns where the value is proven in code
