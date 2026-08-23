# Add a Soft-Delete Pattern with a Global Query Filter

**Category:** Entity Framework Core
**Use when:** Records need to be recoverable/audited rather than hard-deleted.

## Prompt

I need to convert hard deletes on [entity name, e.g. `Customer`] into soft deletes so records are recoverable and auditable, with deleted rows automatically excluded from normal queries. Follow analyze -> propose -> approve -> implement -> test -> review; do not implement until I approve the plan.

Analyze:
1. Locate every place this entity is currently deleted: `DbSet.Remove(...)`, cascade deletes from a parent, or bulk `ExecuteDelete()` calls -- all of them need to change consistently, or the soft-delete guarantee is broken.
2. Check for existing base classes/interfaces (e.g., an `IAuditable`/`ISoftDeletable` pattern already used elsewhere) to reuse rather than inventing a new convention.
3. Identify any unique indexes/constraints on this entity that assume only one live row per key -- a soft-deleted row still occupies that unique value unless the index is adjusted.

Propose:
- Add an `IsDeleted` (bool) and `DeletedAtUtc`/`DeletedBy` set of properties (or reuse the existing audit pattern), configured via `HasQueryFilter(e => !e.IsDeleted)` in `OnModelCreating` or the entity's `IEntityTypeConfiguration<T>`.
- Intercept the actual delete: either override `SaveChanges`/`SaveChangesAsync` in the DbContext (or a `SaveChanges` interceptor) to detect `EntityState.Deleted` and convert it to a modified "soft delete" update instead, or update all call sites directly -- recommend the interceptor approach for consistency and explain the tradeoff of "magic" behavior versus explicit call-site changes.
- If any unique index exists on this entity, propose making it a filtered/partial unique index (`WHERE IsDeleted = 0`) so soft-deleted rows don't block reinserting the same natural key.
- Flag that global query filters are silently bypassed by `IgnoreQueryFilters()` and by raw SQL/`ExecuteDelete`/`ExecuteUpdate` -- any admin/reporting code that must see deleted rows should use `IgnoreQueryFilters()` explicitly, and any bulk operations must be checked for filter bypass side effects.
- Note that navigation properties from other entities to this one will also respect the filter automatically, which may hide related data unexpectedly (e.g., an order whose customer is soft-deleted).

Wait for approval, then implement the model change, migration (including the filtered index if applicable), and the delete-interception logic.

Test: verify a deleted row is excluded from normal queries but still present in the database and visible via `IgnoreQueryFilters()`; verify cascading/related-entity behavior.

Review: confirm every original hard-delete call site was updated and no bulk-delete path bypasses the new soft-delete behavior unintentionally.
