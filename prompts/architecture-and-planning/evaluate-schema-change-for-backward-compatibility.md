# Evaluate a Schema Change for Backward Compatibility

**Category:** Architecture & Planning
**Use when:** A schema change could break an older version of the app still running during a rolling deploy.

## Prompt

I am proposing the database schema change described below. Before anyone runs a migration, evaluate it for backward compatibility with application instances that may still be running the previous code version during a rolling or blue/green deployment. Produce a written evaluation and proposed migration plan for me to approve — do not execute any DDL, migration, or code change yourself.

In your evaluation:

1. **Compatibility analysis** — for each individual change in the proposal (column add/drop/rename/type change, constraint add, index change, table split/merge), state explicitly whether an application instance running the OLD code and an instance running the NEW code can both operate correctly against the changed schema at the same time. Call out any change that is a hard break (e.g., dropping a column still read by old code, renaming a column, tightening a NOT NULL/foreign key constraint, changing a type in a non-widening way).
2. **Expand/contract migration path** — for every change you flag as unsafe to apply directly, propose the expand/contract (parallel-change) sequence instead: an "expand" step that adds the new shape alongside the old and keeps both readable/writable, an intermediate step where application code is deployed to use the new shape while still tolerating the old, and a "contract" step that removes the old shape only after all instances are confirmed running the new code.
3. **Data backfill** — identify what existing data needs to be backfilled or dual-written during the transition window, and how correctness will be verified.
4. **Ordering constraints** — the exact sequence in which schema migration and code deployment must happen, and what breaks if that order is violated.
5. **Rollback plan** — how to safely roll back at each stage if the new code deploy fails, including whether the expand step itself is safely reversible.
6. **Risks and open questions** — anything that depends on details you don't have (e.g., current row counts, whether the column is used in stored procedures/reports outside the app) — list these explicitly rather than assuming.

Present this as a numbered, stepwise plan I can approve stage by stage.
