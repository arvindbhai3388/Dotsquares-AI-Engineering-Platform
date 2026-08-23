# Analyze Missing Indexes and Propose Additions

**Category:** SQL Server
**Use when:** A query is running slowly and no obviously matching index exists.

## Prompt

Analyze the attached slow query (and its workload context if available) to determine whether a missing index is the cause, and propose a concrete fix. Query `sys.dm_db_missing_index_details`, `sys.dm_db_missing_index_group_stats`, and `sys.dm_db_missing_index_groups` scoped to this database, and cross-reference the `equality_columns`, `inequality_columns`, and `included_columns` suggestions against the query's actual predicates, JOIN conditions, and ORDER BY/GROUP BY clauses. Do not blindly apply the DMV's suggested column order — verify selectivity using `sys.dm_db_stats_properties` or a quick `SELECT COUNT(DISTINCT col)` sanity check, and order key columns from most to least selective, equality before inequality.

Pull the actual (not estimated) execution plan and confirm the operator that would benefit (index seek replacing a scan, or a seek replacing key lookups) rather than trusting the missing-index hint alone, since it does not account for existing overlapping indexes or write-heavy tables. Check `sys.indexes` and `sys.dm_db_index_usage_stats` for existing indexes on the table first, and prefer widening/adjusting an existing index (adding INCLUDE columns) over creating a near-duplicate one.

For each candidate index, state: the exact `CREATE INDEX` statement, expected impact on this query's plan, and the write-side cost (extra work on INSERT/UPDATE/DELETE, additional storage, effect on log/lock contention) using `sys.dm_db_index_operational_stats` or table row-count/write-frequency estimates as evidence. Flag any table where write volume is high enough that the trade-off needs a decision, not just a recommendation.

Do not run `CREATE INDEX` yourself. Present the proposed index(es) with the plan-based justification and write-cost trade-offs, and wait for explicit approval before creating anything against a production or production-like database. If you need to test index creation, propose testing on a non-production copy first and say so explicitly.
