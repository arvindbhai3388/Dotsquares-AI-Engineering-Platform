# Review and Fix AsNoTracking Usage on Read-Only Query Paths

**Category:** Entity Framework Core
**Use when:** A read-heavy API is under memory/CPU pressure from unnecessary change tracking.

## Prompt

The [service/repository/controller area] is read-heavy and I suspect the DbContext's change tracker is doing unnecessary work for queries whose results are never modified or saved back. Audit and fix tracking behavior across this area. Follow analyze -> propose -> approve -> implement -> test -> review; show me the full list of affected queries before changing any of them.

Analyze:
1. Enumerate every LINQ query in the target area/files and classify each as read-only (result is returned to a caller, serialized to a DTO/API response, or displayed, and never passed to `SaveChanges`) versus a genuine update path (entity is loaded, mutated, and saved).
2. For each read-only query, confirm it is NOT currently using `AsNoTracking()`/`AsNoTrackingWithIdentityResolution()`, meaning EF Core is snapshotting every returned entity into the change tracker for no benefit.
3. Check whether the DbContext itself is scoped per-request (typical in web apps) so tracked entries don't accumulate across requests, or whether it's long-lived (e.g., a singleton or background worker loop), which makes untracked reads far more urgent to prevent unbounded memory growth.

Propose:
- List each query with the recommended fix: add `.AsNoTracking()` for simple read-only projections/entities, or `.AsNoTrackingWithIdentityResolution()` specifically when the same entity instance appears multiple times in one result graph (e.g., via multiple `Include()`s) and reference equality across that result matters even though nothing will be saved.
- Flag any query currently relying on tracking for change-detection *after* the fact (rare but possible: code that loads without intending to update, then later reuses the same tracked instance to update) -- these need a design review, not just a tag change, since removing tracking would silently break the update.
- If the codebase's default `QueryTrackingBehavior` should be changed context-wide (`ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking` or via `DbContextOptionsBuilder.UseQueryTrackingBehavior`) because the vast majority of queries in this context are read-only, propose that as an alternative to tagging every query individually, and confirm which update paths would then need explicit `.AsTracking()` opt-in.

Wait for my approval on the list and approach.

Implement the approved changes.

Test: confirm all previously-passing tests for update paths still pass (they must still track correctly), and add/observe a memory or query-time comparison if tooling is available.

Review: confirm no update path was accidentally marked no-tracking.
