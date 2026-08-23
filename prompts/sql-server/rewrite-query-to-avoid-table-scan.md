# Rewrite a Query to Replace a Table Scan with an Index Seek

**Category:** SQL Server
**Use when:** An execution plan shows a table or clustered index scan on a large table for what should be a selective query.

## Prompt

Diagnose why the attached query produces a table scan or clustered index scan instead of an index seek, and rewrite it so SQL Server can seek. Start from the actual execution plan (not estimated) and identify the scanned object, the predicate applied as a residual filter on the scan, and the estimated vs. actual row counts (a large discrepancy points to a stale-statistics or cardinality-estimation problem rather than a missing index).

Check the specific patterns that commonly force a scan even when a usable index exists: a function or expression wrapped around the indexed column in the WHERE clause (e.g. `WHERE CONVERT(varchar, OrderDate, 101) = ...` or `WHERE ISNULL(Status,0) = 1`) which is non-sargable; leading-wildcard `LIKE '%value'`; implicit data type conversion between the column and the parameter/literal (check `sys.columns` types vs. the literal/parameter type); an OR across columns that aren't all covered by one index; or a low-selectivity predicate where the optimizer correctly judges a scan cheaper. For each cause found, rewrite the predicate to be sargable (move the function to the constant side, use a range comparison instead of a wrapped date function, avoid implicit conversion by matching parameter types explicitly) rather than only recommending a new index — a rewrite that fixes a non-sargable predicate is often cheaper and more durable than adding an index around a bad predicate.

If, after making the predicate sargable, no suitable index still exists, state that explicitly and propose the index as a secondary recommendation, including the trade-off on write cost for this table.

Show the before/after query text, the before/after plan shape (operator names and estimated cost if available), and explain the mechanism, not just the result. Do not execute the rewritten query against production data to "confirm" the fix — propose the change and the verification method (e.g., run against a non-prod copy, or via `SET SHOWPLAN_XML ON`) and get sign-off before anything touches a production or production-like database.
