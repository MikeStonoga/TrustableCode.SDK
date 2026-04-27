# Business Evidence Conventions

`BusinessEvidence` is the SDK v2 shape for observable business meaning.

It is intentionally not a generic log message. It represents evidence that a business-relevant admission, transition, invariant, side effect, reconciliation, compensation, or audit event happened or was rejected.

## Core Fields

All logging and tracing adapters must use these stable field names:

| Field | Meaning |
| --- | --- |
| `business.evidence.name` | Stable evidence name, such as `OrderPreparationRejectedEvidence`. |
| `business.evidence.kind` | Evidence category from `EvidenceKind`. |
| `business.evidence.message` | Human-readable business explanation. |
| `business.evidence.correlation_id` | Correlation id when available. |
| `business.evidence.observed_at` | UTC timestamp for when the evidence was observed. |
| `business.metadata.*` | Evidence-specific metadata. |

The source of truth for these names is `BusinessEvidenceFields`.

## Evidence Kinds

Current evidence kinds:

- `Transition`
- `InvariantViolation`
- `BoundaryRejection`
- `SideEffect`
- `Reconciliation`
- `Compensation`
- `Audit`

## Severity Mapping For Logs

`LoggerBusinessEvidenceSink` maps business evidence to log levels as follows:

| Evidence kind | Log level |
| --- | --- |
| `InvariantViolation` | `Warning` |
| `BoundaryRejection` | `Warning` |
| all others | `Information` |

This keeps expected business rejections visible without treating every rejected input as an infrastructure error.

## Trace Naming

`ActivitySourceBusinessEvidenceSink` creates activities named:

```text
business.evidence.{EvidenceKind}
```

Examples:

- `business.evidence.InvariantViolation`
- `business.evidence.BoundaryRejection`
- `business.evidence.SideEffect`

## Metadata Guidance

Metadata keys should describe business meaning, not framework implementation.

Good examples:

- `invariant.code`
- `invariant.name`
- `transition.name`
- `transition.current_state`
- `transition.target_state`
- `admission.name`
- `admission.rule.code`
- `side_effect.name`
- `side_effect.status`
- `side_effect.idempotency_key`

Avoid metadata that only repeats technical plumbing, such as class names, method names, queue library names, or serializer details, unless those details directly explain the business evidence.

