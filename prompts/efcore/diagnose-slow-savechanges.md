# Diagnose a Slow SaveChanges Call

**Category:** Entity Framework Core
**Use when:** SaveChanges latency grows disproportionately as more entities are tracked in a unit of work.

## Prompt

`SaveChanges()`/`SaveChangesAsync()` in [name the DbContext/service/unit of work] is getting slower in a way that seems disproportionate to the actual number of rows being written -- I suspect change-tracker overhead (DetectChanges cost scaling with the number of tracked entities) rather than the database write itself. Follow analyze -> propose -> approve -> implement -> test -> review; diagnose before proposing a fix.

Analyze:
1. Confirm where time is actually going: enable EF Core logging (`LogTo` with `LogLevel.Information` or higher, or a stopwatch around the `SaveChanges` call plus around the actual SQL execution reported in logs) to separate "time in DetectChanges/change-tracking" from "time executing SQL against the database."
2. Count how many entities are tracked in the context at the point `SaveChanges` is called -- check for a long-lived DbContext (a loop that keeps loading/attaching more entities into the same context instance without ever disposing/recreating it, or a batch job processing thousands of rows in one unit of work) since `DetectChanges` cost scales with the total number of tracked entities, not just the changed ones.
3. Check whether automatic change detection is being triggered redundantly -- e.g., a loop that calls `SaveChanges()` once per iteration inside a larger loop (each call re-scans the whole growing tracked graph), or code that accesses `ChangeTracker.Entries()` repeatedly, each of which can trigger a `DetectChanges` pass depending on EF Core version and configuration.

Propose, matched to the actual bottleneck found:
- If it's a long-running batch: propose batching into smaller units of work with a fresh (or explicitly cleared via `ChangeTracker.Clear()`) DbContext every N entities, rather than one context accumulating thousands of tracked entities for the whole job.
- If reads earlier in the same context don't need tracking (they're read-only lookups feeding the batch logic): propose `AsNoTracking()` on those so they never enter the tracked graph in the first place (cross-reference the tracking-review prompt).
- If `ChangeTracker.AutoDetectChangesEnabled` is causing repeated redundant scans in a tight loop, propose temporarily disabling it (`context.ChangeTracker.AutoDetectChangesEnabled = false`) around a bulk-attach/bulk-modify block, calling `DetectChanges()` once manually before `SaveChanges()`, and re-enabling it afterward -- flag this as an advanced, easy-to-misuse optimization that needs a clear comment and test coverage since forgetting to detect changes before save can silently lose updates.
- If the actual write volume is large and set-based, propose `ExecuteUpdate`/`ExecuteDelete` instead of tracked entities entirely (cross-reference that prompt) when per-row side effects aren't required.

Wait for approval on the diagnosis and fix before implementing.

Implement the approved change.

Test: benchmark before/after `SaveChanges` timing at a representative batch size, and confirm correctness (no missed updates from disabled auto-detect, no data loss from batching).

Review: confirm the fix addresses the measured bottleneck, not a guessed one -- re-run the same profiling used in Analyze to prove the improvement.
