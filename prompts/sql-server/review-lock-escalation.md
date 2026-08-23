# Investigate Lock Escalation Causing Unexpected Blocking

**Category:** SQL Server
**Use when:** A bulk operation on one table is unexpectedly blocking unrelated-looking queries against the same table.

## Prompt

Investigate whether lock escalation is the cause of the attached blocking incident, where a bulk UPDATE/DELETE/INSERT on a table appears to block other, seemingly unrelated queries against rows the bulk operation shouldn't logically touch. Confirm the mechanism first: SQL Server escalates row- or page-level locks to a table-level (or partition-level, if the table is partitioned) lock once a single statement holds roughly 5,000 or more locks on one object, to reduce lock-manager memory overhead — once escalated, the entire table (or partition) is locked, blocking queries that would otherwise have proceeded against different rows.

Gather evidence rather than assuming: check `sys.dm_tran_locks` during/after the incident (or the extended-events `lock_escalation` event, which reports the statement, the object, and whether escalation was to TABLE or HOBT granularity) to confirm escalation actually occurred and identify the exact statement responsible. Cross-reference with `sys.dm_exec_requests`/blocking-chain output to confirm the blocked queries were waiting on the escalated lock specifically, not on an unrelated resource.

Propose fixes matched to the actual cause: if the bulk operation can be broken into smaller batches (e.g., `DELETE TOP (n) ... WHERE ...` in a loop with a brief pause or checked via `@@ROWCOUNT`, committing each batch), that keeps the per-statement lock count under the escalation threshold; if the table is partitioned and only one partition needs to be touched, confirm partition-level lock escalation isn't still escalating to the whole table due to a missing partition-aligned index; as a last resort, `ALTER TABLE ... SET (LOCK_ESCALATION = DISABLE or AUTO)` can suppress escalation for a specific table, but flag this as a targeted, evidence-backed exception, not a default fix, since disabling escalation trades one risk (blocking) for another (lock-manager memory pressure under heavy concurrent load).

Also check whether the bulk operation is running in a single large transaction that could instead be committed incrementally, reducing both escalation risk and overall lock duration. Present the evidence, the root cause, and the proposed fix with its trade-off. Do not change server/table lock-escalation settings or restructure the bulk job against production yourself — propose it and get approval before applying anything to production.
