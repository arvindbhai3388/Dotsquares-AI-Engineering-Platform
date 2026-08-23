# Review and Choose the Right Transaction Isolation Level

**Category:** SQL Server
**Use when:** A process is either seeing dirty/inconsistent reads or causing excessive blocking, and the isolation level in use hasn't been deliberately chosen.

## Prompt

Review the attached process's data access (stored procedure, transaction block, or ORM/ADO.NET call path) and determine the transaction isolation level currently in effect (check for an explicit `SET TRANSACTION ISOLATION LEVEL`, a connection-level default, or a database-level setting like READ_COMMITTED_SNAPSHOT), then evaluate whether it's actually appropriate for this specific workload rather than assuming the default is fine.

Diagnose the reported symptom first: if the complaint is inconsistent/dirty reads or phantom rows, confirm with `sys.dm_tran_active_transactions`/`sys.dm_exec_sessions` or a repro what's actually being read mid-transaction, and determine whether the fix is tightening isolation (e.g., moving to REPEATABLE READ or SERIALIZABLE for the specific queries that need it, or adding explicit locking hints only where justified) or whether it's actually a race condition in application logic that isolation level alone won't fix. If the complaint is excessive blocking/timeouts, check `sys.dm_exec_requests` and `sys.dm_os_waiting_tasks` for lock wait types, and evaluate whether enabling READ_COMMITTED_SNAPSHOT (RCSI) at the database level would let readers avoid blocking on writers via row versioning — but explicitly call out the trade-offs: increased tempdb version-store usage, the behavior change from blocking to consistent-but-possibly-stale reads, and that RCSI is a database-wide setting affecting every session, not just the one being tuned.

Do not recommend SERIALIZABLE or table-level locking hints as a default fix for blocking — that typically makes contention worse. Do not recommend NOLOCK/READ UNCOMMITTED as a blanket fix either; if it's already in use, flag the correctness risk (dirty reads, skipped/duplicated rows on page splits) explicitly and ask whether that risk is actually acceptable for this specific query's business purpose.

Present the current isolation level, the specific evidence tying it to the reported symptom, and the recommended change with its trade-offs stated plainly. Do not change the database-level RCSI setting or any isolation level in code yourself — propose it and get explicit approval, since a database-wide isolation change affects every consumer and needs sign-off before touching production.
