# Add a Persisted Computed Column with a Supporting Index

**Category:** SQL Server
**Use when:** Queries filter on a derived expression (e.g., `LOWER(Email)`, `DATEDIFF` from a stored date, or a concatenation) rather than a raw column, forcing a scan.

## Prompt

Diagnose the attached query's non-sargable expression-based filter (e.g., `WHERE LOWER(Email) = @email`, `WHERE YEAR(CreatedDate) = @year`, `WHERE FirstName + ' ' + LastName = @fullName`) and fix it by adding a persisted computed column with a supporting index, rather than trying to index the raw column and hoping the optimizer figures out the expression (it generally won't, since the expression itself defeats a standard index on the base column).

Add the computed column with `ALTER TABLE ... ADD ComputedCol AS (<expression>) PERSISTED` — persisted is required (not just a virtual computed column) for the column to be indexable, and confirm the expression is deterministic (SQL Server will reject a `PERSISTED` computed column on a non-deterministic expression, e.g., one using `GETDATE()` without justification) before proposing it. Create a nonclustered index on the new computed column, matching the key/INCLUDE design to the actual query's other predicates and SELECT list, same as any other index design.

Rewrite the application/procedure query to filter on the computed column directly (`WHERE ComputedCol = @email`) instead of wrapping the raw column in the function at query time, since the optimizer will only use the computed column's index when the query expression matches it, either verbatim or via automatic expression matching (verify this in the execution plan rather than assuming a match) — if automatic matching doesn't kick in, the query must be rewritten to reference the computed column by name.

Call out the concrete trade-offs: adding a persisted computed column to a large existing table requires computing and storing the expression for every existing row (a size-of-data operation, and it will briefly hold a schema-modification lock), and every future INSERT/UPDATE that touches the source column(s) now also maintains the computed value and its index — for a high-write table, weigh this against the read benefit. Also flag any backward-compatibility risk: if the underlying expression's logic ever needs to change (e.g., the "normalize email" logic gets more complex), the computed column definition must be dropped and recreated, which requires dropping its dependent index first.

Provide the full DDL and the rewritten query. Do not run the `ALTER TABLE` or `CREATE INDEX` against production. Propose it, note the expected duration/locking impact of the initial computation on this table's row count, and get approval before applying anything to production.
