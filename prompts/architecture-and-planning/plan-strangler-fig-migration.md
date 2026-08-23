# Plan a Strangler-Fig Migration for a Legacy Module

**Category:** Architecture & Planning
**Use when:** A legacy module needs modernizing but a full rewrite is too risky/expensive to do in one step.

## Prompt

Plan a strangler-fig migration strategy for the legacy module described below — incrementally replacing it in place rather than a big-bang rewrite. Produce a phased migration plan for me to review and approve; do not begin moving or rewriting any code yet.

First, read enough of the existing module to identify its real seams: its public entry points (methods, endpoints, events it's called from), its external dependencies (database tables, other services, plugins), and any internal coupling that would make partial extraction hard (shared mutable state, singletons, direct SQL scattered across the module).

Then produce a plan covering:

1. **Current-state map** — the module's boundaries, its callers, and its dependencies, so we know exactly what the "old" side of the strangler facade needs to intercept.
2. **Seams for incremental extraction** — break the module into 3-6 independently migratable slices (by responsibility or by call path, whichever divides more cleanly), ordered by a mix of business value and technical risk (lowest-risk/highest-value slice first).
3. **Facade/routing strategy** — how new code and old code will coexist during the migration: a routing layer, feature flag, or interface abstraction that lets each slice be redirected to the new implementation independently, and how requests are routed to old vs. new per slice.
4. **Per-slice migration steps** — for the first slice specifically, the concrete steps: build the new implementation alongside the old, verify it against the old (e.g., shadow/parallel-run comparison or characterization tests around the old behavior first), cut traffic over, then remove the old code path.
5. **Data considerations** — if the slice touches shared data, whether both old and new implementations need to read/write the same schema during the transition (tie to expand/contract if a schema change is involved).
6. **Risks** — behavioral drift between old and new, incomplete test coverage of the legacy behavior before extraction, and the risk of the migration stalling halfway (old and new coexisting indefinitely).
7. **Rollback plan** — how to revert a single slice's cutover independently without affecting the others.
8. **Effort estimate** — rough sizing per slice and total, with the biggest unknowns called out.

Wait for approval before extracting or moving any code.
