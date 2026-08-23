# SQL Server Guidelines

Guidance for schema design, query authoring, and performance work directly against SQL Server — whether accessed through EF Core (see [EF Core Guidelines](EFCore-Guidelines.md)) or raw ADO.NET, as is the case in some legacy Dotsquares client codebases.

## Indexing strategy

- Every foreign key column should generally have a non-clustered index, unless the table is small/rarely queried by that key — SQL Server does **not** automatically index foreign keys (unlike the primary key, which is indexed by its clustering/unique constraint).
- Design the **clustered index** (usually the primary key, but not necessarily) around the table's dominant access pattern — an ever-increasing key (identity column, or sequential `NEWSEQUENTIALID()` rather than random `NEWID()` for GUID keys) avoids page-split-heavy insert patterns that a random clustering key produces on high-insert-volume tables.
- Use **covering indexes** (`INCLUDE` columns) for hot, narrow, frequently-run queries where the query's `SELECT` list is known and stable — this lets SQL Server satisfy the query entirely from the index without a key lookup back to the clustered index.
- Avoid over-indexing: every additional index speeds reads but slows every `INSERT`/`UPDATE`/`DELETE` that touches indexed columns, and consumes storage. Add an index because a specific, observed query pattern needs it (confirmed via an execution plan, not a guess), not preemptively on every column that might someday be filtered on.
- Watch for **implicit conversions** silently defeating an index — comparing an `nvarchar` column to a non-Unicode literal, or a numeric column compared to a string parameter, can force a scan instead of a seek even with the "right" index in place. Match parameter types to column types exactly.
- Periodically review index fragmentation and missing/unused index DMVs (`sys.dm_db_index_usage_stats`, `sys.dm_db_missing_index_details`) rather than only reacting to a specific slow query — this is normally a DBA/ops concern but is worth flagging when a project has no such process at all.

## Parameterization / injection prevention

- **Every** query with any externally influenced value — user input, a value from a query string, a value from another system's API response — must use parameters. No exceptions, no "it's just an internal admin tool," no "the value is validated upstream so it's fine to concatenate."
- With EF Core (LINQ), parameterization is automatic — the risk surface is `FromSqlRaw`/`FromSqlInterpolated` and raw `ExecuteSqlRaw` calls. Use `FromSqlInterpolated`/`ExecuteSqlInterpolated` with `$"..."` string interpolation syntax (EF Core rewrites the interpolated values into parameters automatically) rather than `FromSqlRaw` with manual string concatenation — never build the SQL text by concatenating a raw value into a `FromSqlRaw`/`ExecuteSqlRaw` string.
- With raw ADO.NET (`SqlCommand`), always use `SqlParameter`/`command.Parameters.Add(...)` (or `.AddWithValue`, though explicit `SqlDbType` is preferred for correctness with numeric/date types) — never `string.Format`/interpolation/concatenation to build a query containing any value that didn't originate as a hardcoded literal in the code itself.
- Dynamic **identifiers** (table names, column names, sort-by column selected from a UI dropdown) cannot be parameterized the way values can — validate them against an explicit allow-list of known-safe identifiers before use, never pass a user-supplied identifier through unchecked, even if it's also being compared against a supposed list elsewhere in the code.
- Stored procedures are not inherently injection-proof — a stored procedure that itself builds and executes dynamic SQL via `EXEC(@sql)` from concatenated input reintroduces the exact same vulnerability one level down; parameterize inside the procedure too.
- Least-privilege the application's SQL login — it should not be `db_owner` in production. Grant only the specific `EXECUTE`/`SELECT`/`INSERT`/`UPDATE`/`DELETE` permissions the application actually needs, ideally scoped through stored procedures/schemas rather than blanket table grants.

## Stored procedures vs. inline queries

Neither is universally "better" — pick based on the actual concern:

| Favors stored procedures | Favors inline/ORM-generated queries |
|---|---|
| Complex, multi-statement logic best kept close to the data (heavy set-based transforms, temp-table-driven batch processing) | Simple CRUD that maps naturally to EF Core LINQ — a hand-written procedure for `SELECT * FROM Orders WHERE Id = @id` adds a deployment artifact for no real benefit |
| A DBA team owns and tunes query plans independently of application deploys | The team wants schema and query logic to travel together in source control and be reviewed as one diff (a stored procedure lives in a separate deployment artifact unless scripted via EF Core migrations or a dedicated SQL project) |
| Need to grant `EXECUTE` permission on a procedure without granting direct table access (a genuine, meaningful security boundary) | Query shape varies significantly based on runtime conditions (optional filters) — dynamic LINQ composition is far more maintainable than a procedure with a dozen `@filter1 IS NULL OR Column1 = @filter1` branches |
| A query is called from multiple different application surfaces (a web app and a batch job) and must behave identically | Rapid iteration during active feature development, where a procedure's separate deployment lifecycle would slow the team down |

Default for new Dotsquares projects: EF Core/inline parameterized queries for standard CRUD and reporting, reserving stored procedures for genuinely complex set-based operations or where an existing client project already standardizes on them — match the existing project's convention rather than introducing a second pattern.

## Execution plan review process

- Before merging any new or materially changed query against a table with non-trivial row counts (a judgment call, but treat "tens of thousands of rows or more" as the threshold worth checking), capture its **actual execution plan** (not just estimated) via SSMS (`Ctrl+M` / "Include Actual Execution Plan") or `SET STATISTICS IO, TIME ON` and inspect it for:
  - **Table/index scans** where a seek was expected — usually a missing or non-covering index, or an implicit conversion defeating one.
  - **Key Lookups** appearing with a high "Estimated Number of Rows" — often resolved by adding the looked-up columns to the index's `INCLUDE` list.
  - **Warnings** on operators (yellow triangle icons in SSMS) — implicit conversion, missing statistics, or spilled sorts/hashes to `tempdb` are all worth investigating, not ignoring.
  - Wildly inaccurate **estimated vs. actual row counts** — a sign of stale statistics or a non-sargable predicate (a `WHERE` clause wrapping the column in a function, e.g. `WHERE YEAR(OrderDate) = 2026`, which prevents an index seek; rewrite as a range predicate instead: `WHERE OrderDate >= '2026-01-01' AND OrderDate < '2027-01-01'`).
- For EF Core-generated SQL specifically, use `ToQueryString()` on the `IQueryable` (or SQL Server Profiler/EF Core logging) to see the actual generated SQL before assuming a LINQ expression translates the way it looks like it should.
- Re-check the execution plan after adding an index — confirm the optimizer actually chooses to use it; an index that "should" help but isn't selected is a strong signal something else (cardinality estimates, an existing better index, a plan guide) is at play.
- For genuinely performance-critical or high-volume queries, this review should happen as part of the [Review](AI-Workflow-Discipline.md) step before the change ships, not discovered later via a production incident.

## Related pages

- [EF Core Guidelines](EFCore-Guidelines.md)
- [Architecture Overview](Architecture-Overview.md)
- [Security Guidelines](../docs/Security-Guidelines.md) — secrets and least-privilege for database credentials.
