# Analyze an Execution Plan for a Slow Query

**Category:** SQL Server
**Use when:** A query needs a deep performance diagnosis beyond a quick "add an index" guess.

## Prompt

Analyze the attached actual execution plan (prefer the actual plan with runtime statistics over an estimated plan — request one captured via `SET STATISTICS XML ON` or from Query Store if the estimated plan is all that's available, and say so if actual data is missing) for the given slow query, and explain the plan operator by operator in order of cost contribution, not left-to-right.

For each of the top 2-3 costliest operators, identify: the operator type and the object/index it touches, estimated vs. actual row counts (flag any large discrepancy, since that signals a cardinality-estimation problem — usually stale or missing statistics, an out-of-date histogram, or a non-sargable predicate — rather than a pure indexing problem), and whether it's spilling to tempdb (check for a Sort or Hash Match warning icon, or `SpillToTempDb` in the XML) which indicates the memory grant was insufficient. Check for implicit conversions flagged in the plan (a warning triangle on a Scan/Seek), parallelism (and whether `CXPACKET`/`CXCONSUMER` waits accompany this query, suggesting the parallel plan itself is a symptom of a missing index rather than a benefit), and any Key Lookup that suggests a covering index would help.

Distinguish between problems fixable by an index, a query rewrite, an updated statistics/index rebuild, or a server/database-level setting (e.g., cost threshold for parallelism, max degree of parallelism, or memory grant configuration) — do not default to "add an index" if the evidence points elsewhere. Where statistics look stale, recommend `UPDATE STATISTICS ... WITH FULLSCAN` (or checking `STATS_DATE()`/`sys.dm_db_stats_properties`) as the first, lowest-risk step before any schema change.

Summarize with a plain-language explanation suitable for a developer who doesn't read plans daily, then list concrete next steps in priority order. Do not apply any of the proposed changes (index creation, statistics updates, configuration changes) yourself — present the analysis and proposed fixes, and get approval before running anything against a production or production-like database.
