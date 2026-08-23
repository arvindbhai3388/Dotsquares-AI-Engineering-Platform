# Add a SaveChanges Interceptor for Audit Fields

**Category:** Entity Framework Core
**Use when:** Audit fields (CreatedAt/ModifiedBy/etc.) are currently being set manually and inconsistently.

## Prompt

Audit fields like `CreatedAtUtc`, `ModifiedAtUtc`, and `ModifiedBy` on [entity or "all entities implementing an audit interface"] are currently being set manually at each call site, and it's inconsistent -- some paths forget to set them. I want a `SaveChanges` interceptor (or `ChangeTracker`-based override) that stamps these automatically and consistently. Follow analyze -> propose -> approve -> implement -> test -> review; confirm the design before implementing.

Analyze:
1. Identify (or propose introducing) a common marker interface, e.g. `IAuditable { DateTime CreatedAtUtc { get; set; } DateTime? ModifiedAtUtc { get; set; } string CreatedBy { get; set; } string ModifiedBy { get; set; } }`, and check which entities should implement it.
2. Determine how the "current user" is obtained in this codebase (`IHttpContextAccessor`, a claims principal service, a background-job's system identity) so the interceptor can resolve "who" without introducing a bad dependency (e.g., a DbContext-layer class should not directly depend on `HttpContext` -- inject an abstraction instead).
3. Confirm whether existing rows/call sites already set these fields correctly in some places -- the interceptor must not overwrite a legitimately different `CreatedAtUtc` on update (only touch it on insert).

Propose:
- Implement `SaveChangesInterceptor` (overriding `SavingChanges`/`SavingChangesAsync`) that iterates `ChangeTracker.Entries<IAuditable>()`, and for `EntityState.Added` sets `CreatedAtUtc`/`CreatedBy`, for `EntityState.Modified` sets `ModifiedAtUtc`/`ModifiedBy` only (never touching `CreatedAtUtc`/`CreatedBy` on update).
- Register it via `optionsBuilder.AddInterceptors(new AuditInterceptor(currentUserService))` in the DbContext configuration, using DI to resolve the current-user abstraction.
- Use `DateTime.UtcNow` (or an injected `TimeProvider`/clock abstraction if one already exists in the codebase, for testability) rather than calling `DateTime.UtcNow` directly inside untestable code.
- Note that this interceptor will NOT run for `ExecuteUpdate`/`ExecuteDelete` bulk operations or raw SQL, since those bypass the change tracker -- flag any existing bulk-operation code paths that also need audit stamping and how they should get it (explicit `SetProperty` calls for the audit columns).

Wait for approval, then implement the interceptor, marker interface, registration, and remove the now-redundant manual stamping at call sites.

Test: verify insert sets Created* fields and leaves Modified* null; verify update sets Modified* fields and leaves Created* fields untouched; verify the current-user resolution is injectable/mockable in tests.

Review: confirm no call site double-stamps (manual code plus interceptor) leading to inconsistent values.
