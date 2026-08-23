# Add a Database Index via Fluent API

**Category:** Entity Framework Core
**Use when:** A WHERE/ORDER BY/JOIN column has no supporting index and queries are scanning.

## Prompt

Queries against [describe the table/columns and the query pattern, e.g. "`Orders` filtered by `CustomerId` and `Status`, ordered by `CreatedAtUtc`"] appear to be doing a table/index scan instead of a seek. I want to add the correct index via EF Core Fluent API. Follow analyze -> propose -> approve -> implement -> test -> review; get my sign-off on the index design before generating a migration.

Analyze:
1. Confirm the exact query shape(s) that need support -- which columns appear in `Where()`, `OrderBy()`, and `Join()`/navigation FK lookups -- since column order in a composite index must match the most selective/most commonly filtered column first, then range/sort columns.
2. Check for existing indexes on this table (including the clustered/primary key and any FK-implicit indexes EF Core already creates) to avoid a redundant or overlapping index.
3. Estimate table size and write frequency -- every index adds write/insert overhead and storage, so an index on a very hot, very large write-heavy table needs to be justified against that cost.

Propose:
- Show the exact Fluent API: `modelBuilder.Entity<Order>().HasIndex(o => new { o.CustomerId, o.Status }).HasDatabaseName("IX_Orders_CustomerId_Status")`, with explicit column order matching the query's filter pattern.
- If the query pattern includes an equality filter that should exclude most rows (e.g., `WHERE IsDeleted = 0` or `WHERE Status = 'Active'`), propose a filtered/partial index (`.HasFilter("[IsDeleted] = 0")`) to keep the index small and effective.
- If uniqueness should also be enforced (not just query performance), propose `.IsUnique()` and confirm this matches an actual business rule, not just a performance want.
- If the index needs to support a covering-index (index-only) query, propose `.IncludeProperties(...)` (SQL Server) to avoid a key lookup for a few extra selected columns.
- Flag that adding an index to a large existing table requires a migration that may lock the table during creation depending on the provider -- reference the safe-migration workflow for online/concurrent index creation if the table is large or high-traffic.

Wait for approval, then implement the Fluent API change and generate the migration, showing the actual Up()/Down() SQL.

Test: verify the new index is created and, if you have access to `EXPLAIN`/execution plan tooling, confirm the target query now uses a seek instead of a scan.

Review: confirm the index doesn't duplicate an existing one and that its write-cost tradeoff was considered.
