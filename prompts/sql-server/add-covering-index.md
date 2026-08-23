# Design a Covering Index to Eliminate Key Lookups

**Category:** SQL Server
**Use when:** A query's execution plan shows an index seek followed by a large number of key lookups (RID or clustered-index lookups) back to the base table.

## Prompt

Design a covering index for the attached query so its execution plan no longer needs a key/RID lookup operator. Start from the actual execution plan: identify the seek predicate's columns (these become the index key, ordered by equality predicates first then range/inequality predicates, most selective first), and separately identify every other column the query references — in the SELECT list, in any additional WHERE/JOIN/ORDER BY/GROUP BY clause not already in the key — which should go in the `INCLUDE` clause rather than the key, since INCLUDE columns are cheaper to maintain and don't affect key-based sort order.

Confirm the lookup is actually the expensive part by checking its estimated/actual cost and execution count in the plan (a lookup executed once per outer row against a large outer set is the classic case; if the outer row count is small, a covering index may not be worth the added write cost). Check `sys.dm_db_index_usage_stats` and existing indexes on the table first — prefer widening an existing near-miss index with additional INCLUDE columns over creating a brand-new duplicate index, since redundant indexes increase write and storage cost without proportional benefit.

Produce the exact `CREATE INDEX ... INCLUDE (...)` statement, and state: the columns chosen for the key vs. INCLUDE and why, the expected plan change (seek only, no lookup), and the write-side cost for this specific table (estimate using its write frequency and row width — a wide INCLUDE list on a hot OLTP table with heavy INSERT/UPDATE traffic is a real trade-off worth flagging explicitly, not glossing over).

Do not create the index yourself. Present the proposed index with the before/after plan reasoning and the write-cost trade-off, and wait for explicit approval before running `CREATE INDEX` against a production or production-like database — recommend `CREATE INDEX ... WITH (ONLINE = ON)` if the edition/table size warrants avoiding a blocking index build.
