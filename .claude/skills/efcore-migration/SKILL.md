---
name: efcore-migration
description: >
  Use when a change requires an EF Core schema change — adding/removing/
  renaming a column or table, changing a relationship, adding a
  constraint or index. Trigger phrases: "add an EF Core migration",
  "change this entity's schema", "add a column to this table via EF
  Core", "how do I do this migration safely". Enforces an expand/contract
  approach so schema changes are safe when the old and new application
  versions may run concurrently (rolling deployments, multiple app
  instances mid-deploy).
---

# EF Core Migration Workflow (Expand/Contract)

Any schema change that a currently-running previous version of the app
still reads or writes is a **breaking** change if done in one step. This
skill enforces expand/contract: make the new shape additive and
compatible first, migrate data, only remove the old shape once nothing
depends on it.

## Step 0 — Decide if expand/contract is actually needed

Not every migration needs multiple phases:

- **Safe as a single migration**: adding a new nullable column, adding a
  new table, adding a new index (online in supported SQL Server editions),
  widening a column's length/precision.
- **Needs expand/contract**: renaming a column/table, changing a column's
  type in an incompatible way, adding a **non-nullable** column to a
  table with existing rows, removing a column/table still read by any
  currently-deployed app version, splitting/merging tables, changing a
  relationship's cardinality.

If genuinely single-instance/single-deploy with guaranteed downtime and
no concurrent version overlap, a simpler single-step migration may be
acceptable — but default to expand/contract unless that's explicitly
confirmed, since assuming zero-downtime deployment is the safer default
for a platform used across many client projects with varying ops
maturity.

## Step 1 — Expand

Add the new shape alongside the old one, without removing anything yet:

- New column: add it **nullable** (or with a `DEFAULT` that's valid for
  existing rows) even if the long-term intent is non-nullable — a
  non-nullable column added directly to a populated table either fails
  the migration (no default) or silently backfills every existing row
  with the same default value, which may not be semantically correct.
- Renaming: add the **new** column/table alongside the old one rather
  than renaming in place — a rename is really "add new, migrate data,
  drop old" split across phases, not a single atomic operation, because
  the old app version needs the old name to keep working during rollout.
- New relationship/table: add it as a genuinely new addition; don't
  simultaneously remove what it replaces.
- Generate the migration (`dotnet ef migrations add ExpandXyz`) and
  **read the generated `Up`/`Down` methods before applying** — EF Core's
  scaffolding sometimes generates a drop+recreate for what looks like a
  simple rename, which loses data; if that happens, hand-edit the
  migration to an explicit `RenameColumn`/`RenameTable` (or a
  add-then-copy sequence) instead of accepting the generated drop.

## Step 2 — Deploy application code that writes both shapes (if applicable)

For a rename/restructure, ship an application version that:

- Writes to both the old and new column/table (dual-write) during the
  transition window, or
- Reads from the new shape with a fallback to the old shape if the new
  one isn't populated yet.

This is what actually makes the migration zero-downtime — the schema
change alone isn't sufficient if in-flight old-version app instances
still expect the old shape to be authoritative.

## Step 3 — Backfill existing data

- Write a data-migration step (either inside the EF Core migration's
  `Up()` via raw SQL/`migrationBuilder.Sql(...)`, or a separate one-off
  script) to populate the new column/table from the old one for existing
  rows.
- For large tables, batch the backfill (chunked updates) rather than a
  single unbounded `UPDATE` that could hold long locks — check the
  table's expected row count before assuming a single-statement backfill
  is safe.
- Verify the backfill actually completed and matches expectations (spot-
  check row counts / sample values) before proceeding to Contract.

## Step 4 — Contract

Only after the new shape is fully populated and **all** currently-deployed
app instances are confirmed running the version that no longer depends on
the old shape:

- Switch application code to read/write only the new shape (remove the
  dual-write/fallback from Step 2).
- Generate a follow-up migration that drops the old column/table
  (`dotnet ef migrations add ContractXyz`), and review its generated SQL
  before applying — confirm it drops only the intended old artifact.
- Do not combine Expand and Contract into a single migration/deployment
  when any version-overlap risk exists — that reintroduces exactly the
  breaking-change risk this workflow avoids.

## Throughout

- Never hand-edit the database schema outside of migrations — this
  causes `dotnet ef database update`/future migrations to diverge from
  the model snapshot and produces confusing, hard-to-diagnose drift.
- Never share a `DbContext` instance across concurrent migration/backfill
  operations run in parallel — it isn't thread-safe.
- Add/verify a concurrency token on any entity affected if concurrent
  edits during the transition window are plausible.
- Test each phase's migration against a representative copy of
  production-shaped data (row counts, nulls, edge-case values) where
  feasible, not just an empty dev database — an empty database hides
  backfill/default-value problems entirely.

## Do
- Default to expand/contract unless single-step safety is explicitly
  confirmed.
- Review every generated migration's `Up`/`Down` before applying it.
- Backfill in batches for large tables.
- Confirm old-version app instances are fully drained before Contract.

## Don't
- Don't add a non-nullable column to a populated table without a valid
  default or a prior backfill.
- Don't combine expand and contract into one migration when version
  overlap is possible.
- Don't hand-edit the schema outside of migrations.
- Don't apply a migration to production without having reviewed its
  generated SQL first.
