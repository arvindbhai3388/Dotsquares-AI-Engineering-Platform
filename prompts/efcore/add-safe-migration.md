# Add a Safe EF Core Migration for a Schema Change

**Category:** Entity Framework Core
**Use when:** Adding or changing a column or table on an existing production database.

## Prompt

I need to change the schema for [describe the entity/table and the change, e.g. "add a required NotNull `Status` column to the `Orders` table" or "rename `CustomerName` to `FullName`"]. Before writing any code, follow the analyze -> propose -> approve -> implement -> test -> review workflow.

Analyze phase:
1. Locate the entity class and its Fluent API configuration (OnModelCreating or IEntityTypeConfiguration<T>).
2. Estimate the table's row count/criticality from context (config, docs, or ask me if unknown) and flag if this table is large or high-traffic.
3. Identify whether the change is additive (safe) or destructive (rename/drop/type change/adding NOT NULL without a default) and call out the risk class explicitly.

Propose phase:
- Show me the exact model/Fluent API change and the `dotnet ef migrations add <Name>` command you intend to run.
- If adding a NOT NULL column to an existing table, propose the expand/contract pattern: add nullable first, backfill via a data migration step or raw SQL in the Up() method, then add the NOT NULL constraint in a follow-up migration.
- If renaming a column/table, propose `RenameColumn`/`RenameTable` explicitly rather than a drop+add pair, to avoid data loss and preserve any FK/index dependencies.
- Flag whether an index needs to be created `CONCURRENTLY`/`ONLINE` (provider-dependent) to avoid locking the table during creation, and whether the migration should be wrapped in a transaction or run outside one for that reason.
- Note any default value, check constraint, or FK behavior that migration scaffolding might get wrong and needs manual editing in Up()/Down().

Wait for my explicit approval of the plan before generating the migration.

Implement: generate the migration, then show me the full Up() and Down() methods (not just a summary) for review, including whether Down() is actually reversible.

Test: verify the migration applies cleanly to a fresh database and, if possible, against a copy of representative data, and confirm the model snapshot matches.

Review: confirm no unrelated model changes leaked into the migration diff.
