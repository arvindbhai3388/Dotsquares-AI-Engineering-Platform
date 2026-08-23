# Add Seed Data via HasData or a Seeding Method

**Category:** Entity Framework Core
**Use when:** Reference/lookup data needs to ship with the schema (e.g., status codes, roles, countries).

## Prompt

I need to seed [describe the data, e.g. "a fixed set of `OrderStatus` lookup rows"] so it ships with the schema and is present in every environment. Use analyze -> propose -> approve -> implement -> test -> review, and get my approval on the approach before writing the seed data.

Analyze:
1. Determine whether this is truly static, never-changing-by-users reference data (a good fit for `HasData` in `OnModelCreating`) versus data that might be edited later by an admin or differ per environment (a better fit for a runtime seeding method run at startup, e.g. via `context.Database.Migrate()` + a conditional insert, or a dedicated `IHostedService`/seeding class).
2. Check whether the entity's primary key is database-generated (identity) — `HasData` requires explicit, stable key values, so identity columns need fixed seed IDs supplied.
3. Check for any existing seeding pattern already used in this codebase and follow it rather than introducing a second one.

Propose:
- If using `HasData`: show the exact `modelBuilder.Entity<T>().HasData(...)` call with explicit primary keys, and note it will generate a migration that inserts/updates/deletes rows to match the exact data set on every model change (i.e., it's declarative, not just "run once" -- removing an item from the list generates a migration that deletes it).
- If using a runtime seeding method: show idempotent logic (check-then-insert or `Any()`/upsert) so re-running it doesn't duplicate rows, and identify where in startup/composition it should be invoked.
- Note any FK dependency order (seed parent lookups before children referencing them).

Wait for approval.

Implement the chosen approach and generate the migration if using `HasData`.

Test: verify a fresh migration/seed run produces exactly the expected rows, and that re-running migrations/seeding (idempotency check) does not duplicate or error.

Review: confirm the seed data doesn't leak environment-specific or sensitive values into a migration that will be committed to source control.
