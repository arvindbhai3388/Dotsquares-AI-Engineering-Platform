# Replace Load-and-Loop Bulk Operations with ExecuteUpdate/ExecuteDelete

**Category:** Entity Framework Core
**Use when:** A bulk update/delete is currently implemented by loading and looping over entities.

## Prompt

The method [name it, e.g. "archive all orders older than a cutoff date"] currently loads entities into memory with a query, loops over them setting properties (or calling `Remove`), and then calls `SaveChanges()`. I want to replace this with EF Core's `ExecuteUpdate`/`ExecuteDelete` for a single set-based database operation, since the current approach pulls potentially large row counts into memory and tracks them all unnecessarily. Follow analyze -> propose -> approve -> implement -> test -> review; confirm the approach before changing code.

Analyze:
1. Confirm the operation is genuinely set-based (the same update/delete logic applies uniformly to every matching row, with no per-row business logic, side effects, or domain events that need to fire per entity) -- `ExecuteUpdate`/`ExecuteDelete` bypasses the change tracker entirely, so no `SaveChanges` interceptors, audit-field interceptors, domain events, or `IEntityTypeConfiguration`-level triggers written as C# will run.
2. Check for any concurrency tokens on the entity -- `ExecuteUpdate`/`ExecuteDelete` do not check optimistic concurrency tokens the way `SaveChanges` does, so confirm this is acceptable for this operation or add an explicit `Where` clause guard.
3. Check for cascade-delete relationships that depend on EF's in-memory cascade behavior -- `ExecuteDelete` relies on database-level cascade behavior (FK ON DELETE CASCADE) rather than EF's tracked-graph cascade, so verify the DB constraints actually match the intended cascade semantics.

Propose:
- Show the exact replacement: `await context.Orders.Where(o => o.CreatedAtUtc < cutoff).ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Archived"))` or `.ExecuteDeleteAsync()`, translated directly to a single SQL statement.
- Note this executes immediately against the database (no `SaveChanges` call, no staging in the change tracker) -- flag any code that currently batches this update with other pending changes in the same `SaveChanges()` call, since ordering/transaction semantics change.
- If per-row side effects (audit log entries, domain events) are actually required, propose wrapping the `ExecuteUpdate` in an explicit transaction alongside a separate batched insert of audit rows, rather than reverting to the loop.

Wait for approval, then implement the change.

Test: verify the correct rows are affected (a scoped `Where` matching exactly the intended set), and confirm any downstream logic expecting `SaveChanges` interceptor side effects (audit stamps, events) still gets them via the alternative path or is documented as intentionally skipped.

Review: confirm the missing change-tracker side effects were explicitly considered, not overlooked.
