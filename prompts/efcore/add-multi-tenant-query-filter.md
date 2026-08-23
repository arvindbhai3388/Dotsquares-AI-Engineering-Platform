# Add a Multi-Tenant Global Query Filter

**Category:** Entity Framework Core
**Use when:** A shared-database multi-tenant model risks cross-tenant data leakage.

## Prompt

We use a shared-database multi-tenant model and I need every query against [entity or "all tenant-scoped entities"] to be automatically scoped to the current tenant, so a bug in one query path can't leak another tenant's data. Follow analyze -> propose -> approve -> implement -> test -> review; treat this as security-critical and confirm the design before implementing.

Analyze:
1. Identify every entity that carries a `TenantId` (or equivalent) and confirm whether it's a normal CLR property or should be a shadow property (see the shadow-property prompt if it should be infrastructure-only).
2. Determine how the current tenant is resolved at query time (claims principal, request header, a scoped `ICurrentTenantService`) and confirm this resolution happens BEFORE the DbContext is constructed or is available via a scoped service injected into the DbContext constructor -- `HasQueryFilter` expressions are evaluated per-query using values captured at model-configuration/context-construction time, so the tenant resolution mechanism must be compatible with that.
3. Check for any existing raw SQL, `ExecuteUpdate`/`ExecuteDelete`, or `IgnoreQueryFilters()` usage against tenant-scoped entities -- all of these bypass the filter and are the most likely sources of a real leak, so they need explicit manual tenant scoping.
4. Check for background/system contexts (migrations, scheduled jobs, admin tooling) that legitimately need cross-tenant access, so the design doesn't accidentally block them or force an unsafe global bypass.

Propose:
- Configure `modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId)` for every tenant-scoped entity, ideally via a shared base configuration or a loop over entities implementing an `ITenantScoped` marker interface rather than repeating it per entity.
- Inject the tenant-resolution service into the DbContext constructor as a scoped dependency, and cache the tenant ID once per context instance (not re-resolved per query) unless the design explicitly needs it to change mid-context-lifetime.
- For legitimate cross-tenant needs (admin tooling, background jobs), propose an explicit, clearly-named escape hatch (e.g., a separate `AdminDbContext` or an explicit `IgnoreQueryFilters()` call gated behind an authorization check), rather than a global toggle that's easy to misuse.
- Flag every raw SQL / `ExecuteUpdate`/`ExecuteDelete` call site found in the analysis step and propose adding an explicit `WHERE TenantId = ...` to each, since the query filter won't protect them.
- If using DbContext pooling, cross-reference the pooling prompt: tenant ID must never be cached in a way that leaks across pooled instances.

Wait for approval given the security sensitivity of this change.

Implement the query filter, tenant resolution wiring, and fixes to any bypassing raw-SQL/bulk-operation call sites found.

Test: write a test with two tenants' data seeded in the same database, and assert that querying as tenant A never returns tenant B's rows, including through navigation properties and the previously-bypassing raw SQL/bulk paths now fixed.

Review: confirm every entity that should be tenant-scoped actually has the filter applied, and that no bypass was left unguarded.
