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

See `docs/implementation-plan.md` for the living plan and status.

See `docs/evidence-conventions.md` for the logging and tracing field conventions used by `BusinessEvidence`.

See `docs/testing-helpers.md` for framework-neutral checks used to test transitions, admissions, invariants, side effects, and evidence.
