# Add a Shadow Property for Infrastructure-Only Metadata

**Category:** Entity Framework Core
**Use when:** A column is needed purely for EF/infrastructure purposes, not domain logic (e.g., a foreign key or tenant ID that shouldn't pollute the domain model).

## Prompt

I need a column on [entity name] purely for infrastructure purposes -- [describe it, e.g. "a `TenantId` used only for the multi-tenant query filter" or "a foreign key to a join table that the domain model shouldn't expose as a CLR property"] -- without adding a CLR property to the domain entity class, since the domain model should stay clean of persistence-only concerns. Use analyze -> propose -> approve -> implement -> test -> review; confirm the approach before implementing.

Analyze:
1. Confirm this genuinely shouldn't be a normal CLR property -- shadow properties are appropriate for FK columns the domain doesn't need to reference directly, or for infrastructure metadata (tenant id, row-level security tag) that would leak persistence concerns into the domain model if exposed as a public property.
2. Check whether the value needs to be read/written from application code at all (if code needs to set it explicitly per operation, a shadow property is awkward -- a real property or a value object might fit better) versus being fully derived/set by EF or an interceptor (a better fit for shadow properties).
3. Confirm the EF Core version in use supports the specific shadow-property + query-filter or shadow-property + interceptor pattern being proposed (behavior has evolved across EF Core versions).

Propose:
- Show the exact configuration: `modelBuilder.Entity<T>().Property<Guid>("TenantId")`, plus how the value gets set -- typically via `context.Entry(entity).Property("TenantId").CurrentValue = ...` in a `SaveChanges` interceptor or override, since there's no compile-time-checked property to assign directly.
- If used for a query filter (e.g., multi-tenancy), show `HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == _currentTenantId)` using `EF.Property<T>()` to reference the shadow property in LINQ.
- Note the debugging/maintainability cost explicitly: shadow properties aren't visible on the class, can't be accessed with IntelliSense/compile-time safety, and are easy for future developers to miss -- recommend a clear comment in the entity configuration explaining why the property is shadow rather than a plain field.
- Confirm the migration will add the column with correct type/nullability/default.

Wait for approval, then implement the configuration, the value-setting mechanism (interceptor or explicit code), and the migration.

Test: verify the shadow property is correctly persisted and correctly filters/joins as intended (e.g., via `context.Entry(x).Property("TenantId").CurrentValue` assertions in a test).

Review: confirm the shadow property doesn't silently need to be set by every insert path -- missing it should fail loudly (e.g., a non-nullable column with no default) rather than silently storing a wrong/default value.
