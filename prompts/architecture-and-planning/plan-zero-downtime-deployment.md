# Plan a Zero-Downtime Deployment for a Breaking Change

**Category:** Architecture & Planning
**Use when:** A change can't simply be deployed all-at-once without a service interruption or version mismatch window.

## Prompt

Plan a zero-downtime deployment strategy for the breaking change described below (API contract change, schema change, or both), given that this will roll out across multiple instances/nodes that will not all update at the same instant. Produce a stepwise deployment plan for me to approve — do not deploy, migrate, or change any code as part of this task.

In the plan, address:

1. **What actually breaks** — identify precisely what fails if old and new versions run simultaneously against the same data/contract: e.g., old clients calling a renamed/removed API field, old code writing rows the new code can't parse, new code requiring a column old code doesn't populate.
2. **Version compatibility window** — during a rolling deploy, both old and new code will be live at once for some period; state how long that window realistically is for this deployment pipeline and what must remain compatible for that entire window.
3. **API contract changes** — if the API is changing, use additive-first versioning: introduce new fields/endpoints alongside old ones, have clients migrate to the new contract, and only remove the old contract in a later, separate deployment once nothing depends on it. Call out any change that cannot be made additive and needs an explicit API version bump instead.
4. **Schema changes** — if a schema change is involved, apply expand/contract: the schema migration (expand) ships and is compatible with both old and new app code, the app code deploy happens next, and the old-shape cleanup (contract) ships only after full rollout is confirmed. State the exact ordering of migration-vs-deploy steps.
5. **Deployment sequencing** — the concrete order of operations (migration, config change, code deploy per service, feature flag flip), and which steps are safe to run in parallel versus must be sequential.
6. **Health checks and verification** — what to verify at each stage before proceeding to the next (smoke tests, canary instance, error-rate monitoring) and the criteria for proceeding versus halting.
7. **Rollback plan per stage** — for each stage, whether it can be rolled back independently, and what rolling back a later stage (e.g., app code) implies for an earlier stage already applied (e.g., schema).
8. **Communication/coordination needs** — any manual step or cross-team coordination required (e.g., notifying API consumers of the deprecation window).

Present this as a numbered sequence of deployable stages, each independently reviewable.
