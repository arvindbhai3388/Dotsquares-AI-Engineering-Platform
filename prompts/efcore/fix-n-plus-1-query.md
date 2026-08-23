# Diagnose and Fix an N+1 Query Pattern

**Category:** Entity Framework Core
**Use when:** A page or endpoint issues far more database round trips than expected.

## Prompt

The endpoint/method [name the controller action, service method, or Razor page] is slow and I suspect an N+1 query problem: EF Core is issuing one query per row instead of a single batched query. Follow analyze -> propose -> approve -> implement -> test -> review; do not change code until I approve the plan.

Analyze:
1. Read the method and trace every place a navigation property is accessed inside a loop (e.g. `foreach (var order in orders) { var lines = order.OrderLines; }`) or accessed lazily after the DbContext query already executed.
2. Confirm whether lazy loading is enabled (proxies) or whether the pattern is a manual re-query per iteration (e.g. calling `_context.Set<T>().Where(...)` inside a loop).
3. If you have access to logs or can enable EF Core's query logging (`LogTo`/`EnableSensitiveDataLogging` in a dev context only), confirm the actual query count matches the row count, and paste the generated SQL for the "before" state.

Propose:
- Recommend eager loading via `.Include()`/`.ThenInclude()` for the specific navigation chain needed, OR a projection with `.Select()` into a DTO that pulls only the required columns, whichever fits the existing code style and avoids over-fetching.
- If multiple sibling collections are being included (risking a cartesian-explosion result set), recommend `AsSplitQuery()` instead and explain the tradeoff (multiple round trips but no row duplication).
- If the query is read-only, also recommend `AsNoTracking()`/`AsNoTrackingWithIdentityResolution()` as appropriate.
- Note any behavior change: e.g., a previously lazy-loaded null/empty case must still be handled after switching to eager loading.

Wait for approval, then implement the minimal change to the query, avoiding unrelated refactors.

Test: write or update a test that asserts on the *number of queries* if the test infrastructure supports it (e.g., a counting interceptor or `DbCommandInterceptor`), plus a normal correctness test. Validate manually that the new SQL is a fixed, small number of round trips regardless of row count.

Review: confirm no regression in eagerly-loaded data volume and no duplicate joined rows.
