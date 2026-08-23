# EF Core Guidelines

Guidance for Entity Framework Core usage across client projects on this platform. This is the platform's standard ORM for **new** .NET projects — note that some existing Dotsquares client codebases (e.g., legacy ASP.NET MVC 5 solutions) may instead use EF6 Database-First or raw ADO.NET; always check a project's own `CLAUDE.md`/conventions before assuming EF Core applies (see [FAQ](../docs/FAQ.md)).

## Migration strategy — expand/contract

Never make a breaking schema change in a single migration when the change needs to be safe across a deploy window where old and new application code (or multiple app instances during a rolling deploy) might both be running against the same database. Use the **expand/contract** pattern:

1. **Expand** — add the new column/table/constraint as purely additive. A new column is nullable or has a default; a renamed column is *added* as a new column, not renamed in place; a new required relationship is added as optional first.
2. **Migrate data** — backfill the new structure from the old one, either in the migration itself (for small tables) or via a separate data-migration step/background job (for large tables, to avoid long locks).
3. **Cut over** — deploy application code that reads/writes the new structure. At this point both old and new columns may still exist.
4. **Contract** — once all application instances are confirmed running the new code (and, if applicable, once a rollback window has passed), a follow-up migration removes the old column/table/constraint.

Concretely: renaming `Orders.CustomerName` to `Orders.ClientName` is **two migrations**, not one `RenameColumn` — add `ClientName`, backfill, deploy code using `ClientName`, then drop `CustomerName` in a later migration/release. A single-migration rename works fine in a dev environment with one app instance and no concurrent deploy, which is exactly why it's an easy trap: it looks correct until it meets a real deployment.

### Migration hygiene

- One logical schema change per migration; do not batch unrelated changes into one migration file, which makes a bad migration harder to isolate and roll back.
- Always review the generated migration's `Up`/`Down` methods — EF Core's diff is usually right but not infallible, especially around renames (which it may model as drop+add, losing data, unless you hand-edit to `RenameColumn`/`RenameTable`).
- Never edit a migration that has already been applied to any shared environment (staging, production) — add a new migration instead, the same "never rewrite shared history" principle as git.
- Name migrations descriptively (`AddClientNameToOrders`, not `Migration1` or `Update`).
- Review `dotnet ef migrations script` output for destructive operations (`DROP COLUMN`, `DROP TABLE`) before it ever runs against a shared database.

## Query performance rules

- **Avoid N+1 queries.** Use `.Include()`/`.ThenInclude()` for related data known to be needed, or `.Select()` into a projection that shapes exactly the fields required — do not iterate a collection and query a related entity per item (`foreach (var o in orders) { var c = await _db.Customers.FindAsync(o.CustomerId); }` is the canonical N+1 bug).
- **Project early.** Prefer `.Select(x => new OrderSummaryDto { ... })` over materializing full entities and mapping afterward, when only a subset of fields is needed — this lets EF Core generate a narrower SQL `SELECT` instead of pulling every column.
- **`AsNoTracking()` for read-only queries.** Any query whose results will not be modified and saved back should use `AsNoTracking()` (or the context-wide `QueryTrackingBehavior.NoTracking` default for read-heavy contexts) to skip EF Core's change-tracking overhead.
- **Beware client-side evaluation.** A LINQ expression EF Core cannot translate to SQL used to silently fall back to in-memory evaluation in EF Core 2.x; EF Core 3.0+ throws at runtime instead for most cases — but a `.ToList()`/`.AsEnumerable()` placed too early in a query chain still forces the rest of the pipeline to run client-side. Watch for this specifically when a query "works" but is unexpectedly slow.
- **Avoid unbounded queries.** Any query returning a collection to a UI or API should be paged (`.Skip()`/`.Take()`, or keyset pagination for large tables) rather than loading an entire table into memory.
- **Split queries for multiple collection includes.** Including more than one collection navigation in a single query (`.Include(o => o.Items).Include(o => o.Notes)`) produces a cartesian-product join by default; use `.AsSplitQuery()` when that duplication would be significant, after confirming with an actual execution plan (see [SQL Server Guidelines](SQL-Server-Guidelines.md)) that it's the better trade-off for that specific query.
- **Compiled queries** (`EF.CompileAsyncQuery`) are a targeted optimization for extremely hot, simple, repeatedly-executed query shapes — not a default; measure before reaching for this.

## `DbContext` scoping

- Register `DbContext` as **Scoped** (the default for `AddDbContext`) — one instance per request/unit of work. Never register it as Singleton; `DbContext` is explicitly not thread-safe, and a shared instance across concurrent requests will corrupt its internal change tracker or throw `InvalidOperationException` for concurrent use.
- Never share one `DbContext` instance across parallel `Task`s/threads. If a background job needs to do concurrent DB work, resolve a **new scope** (and therefore a new `DbContext`) per parallel branch via `IServiceScopeFactory.CreateScope()`, or run the DB operations sequentially.
- For long-running background services (`IHostedService`/`BackgroundService`), do not inject `DbContext` directly into the service's constructor (its lifetime as a singleton-hosted service would capture a single, long-lived `DbContext` instance) — inject `IDbContextFactory<T>` or `IServiceScopeFactory` and create a fresh, short-lived context per unit of work instead.
- Keep a `DbContext`'s lifetime as short as the operation it's serving — do not hold one open across an entire multi-step wizard's UI session (a common temptation in Blazor Server, where a component's lifetime can span many minutes); query, save, and dispose within each discrete operation instead.
- Use `IDbContextFactory<TContext>` specifically in Blazor Server components, since a component instance's lifetime does not map cleanly to "one request" the way a controller action does.

## Concurrency handling

- Use a **concurrency token** (a `rowversion`/`timestamp` SQL Server column mapped as `[Timestamp]` or configured via `.IsRowVersion()`) on any entity subject to concurrent edits, so EF Core can detect a conflicting update and throw `DbUpdateConcurrencyException` rather than silently overwriting another user's change ("last write wins" data loss).
- Handle `DbUpdateConcurrencyException` deliberately at the point a save happens — typical resolutions are "reload and ask the user to reapply their change," "merge non-conflicting fields," or "reject with a clear error" — never a blanket catch-and-retry-the-same-save, which just repeats the original conflict.
- For high-contention counters/aggregates (e.g., inventory stock decrement), prefer an atomic SQL-side operation (`UPDATE Inventory SET Stock = Stock - @qty WHERE Id = @id AND Stock >= @qty`, checking rows-affected) over a read-modify-write through EF Core's change tracker, which is inherently racy under load even with a concurrency token, since the token only detects the conflict after the fact rather than preventing it.
- Choose transaction isolation deliberately for multi-statement operations that must be atomic — wrap them in `await using var tx = await _db.Database.BeginTransactionAsync();` rather than relying on `SaveChangesAsync()`'s own implicit transaction when more than one `SaveChangesAsync()` call, or a mix of EF Core and raw ADO.NET calls, must succeed or fail together.

## Related pages

- [SQL Server Guidelines](SQL-Server-Guidelines.md) — what happens underneath EF Core-generated SQL.
- [Architecture Overview](Architecture-Overview.md) — where the data-access layer sits.
- [C# Coding Standards](Coding-Standards-CSharp.md) — DI lifetime rules referenced above.
