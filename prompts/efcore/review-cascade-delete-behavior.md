# Review and Adjust Cascade Delete Behavior

**Category:** Entity Framework Core
**Use when:** Deleting a parent entity is cascading further than intended, or a required FK is misconfigured as Restrict/blocking legitimate deletes.

## Prompt

[Describe the symptom: e.g. "Deleting a `Customer` is unexpectedly deleting all their `Orders` and `Invoices`" or "We can't delete a `Category` even when it should be safe to, because EF/SQL is blocking it with an FK constraint error."] Review and correct the cascade delete configuration for [relationship/entities involved]. Follow analyze -> propose -> approve -> implement -> test -> review; confirm the intended behavior with me before changing any configuration.

Analyze:
1. Locate the relationship configuration (Fluent API `.OnDelete(...)` or the convention-derived default) for every FK relationship touching the entity in question, in both directions (as parent and as child of other relationships).
2. Recall/confirm EF Core's default convention: required (non-nullable FK) relationships default to `Cascade`, and optional (nullable FK) relationships default to `ClientSetNull` (or `Restrict` at the database level with EF nulling the FK in memory) -- identify which default is currently in effect and whether it was ever set explicitly or is just the unexamined convention.
3. Map out the full dependency chain from the entity in question (parent -> children -> grandchildren) to understand the blast radius of a cascade, since a chain of cascades can silently delete far more than the immediate child table.

Propose, per relationship, one of:
- `DeleteBehavior.Cascade` -- only where deleting the parent should genuinely delete dependents automatically (e.g., an `Order`'s `OrderLines` have no independent meaning without the order).
- `DeleteBehavior.Restrict` -- where a dependent must be explicitly handled/reassigned/deleted first, and the database should refuse the parent delete rather than silently cascading (e.g., a `Category` referenced by live `Products` should block deletion, or require the caller to reassign/delete products first).
- `DeleteBehavior.SetNull` -- where the FK is genuinely optional and orphaning the child (nulling the FK) is the correct business behavior, confirming the FK column is nullable.
- `DeleteBehavior.NoAction` -- rarely correct; only when cascade behavior is being deliberately handled at the application level to avoid multiple-cascade-path SQL Server errors (multiple cascade paths to the same table are disallowed and force `NoAction`/`Restrict` on one of them).
- For each change, note whether it requires a migration (changing FK constraint DDL) and whether existing data would violate the new constraint (e.g., switching to `Restrict` when orphaned rows already exist needs a data cleanup step first).

Wait for approval on the desired behavior per relationship before implementing.

Implement the configuration changes and migration.

Test: write tests exercising each changed relationship's delete path (successful cascade, blocked restrict throwing `DbUpdateException`, correct null-out) to pin down the intended behavior going forward.

Review: confirm the full cascade chain was reviewed, not just the single relationship originally reported.
