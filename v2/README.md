# TrustableCode.SDK v2

`v2` is the next SDK design for making the discipline of *Trustable Code* practical in codebases maintained by developers and AI coding agents.

The first focus is not plumbing. It is semantic orientation:

- what state matters
- which transitions are legitimate
- which invariants must survive change
- which boundaries admit meaning
- which side effects require governance
- which evidence proves behavior after deployment

The current starting package is:

- `TrustableCode.SDK.TrustableModeling`

The current samples are:

- `samples/TrustableCode.SDK.Samples.Ordering`
- `samples/TrustableCode.SDK.Samples.Ordering.Api`

See `docs/implementation-plan.md` for the living plan and status.

See `docs/application-service-pattern.md` for the recommended application-layer flow using admission, governed transitions, evidence, and side-effect lifecycle.

See `docs/persisted-application-flow.md` for the recommended persisted aggregate flow using snapshots, rehydration, save, outbox, and evidence.

See `docs/evidence-conventions.md` for the logging and tracing field conventions used by `BusinessEvidence`.

See `docs/testing-helpers.md` for framework-neutral checks used to test transitions, admissions, invariants, side effects, and evidence.
