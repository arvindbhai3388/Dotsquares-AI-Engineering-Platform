# Investigate a Deadlock from the Deadlock Graph

**Category:** SQL Server
**Use when:** An application is intermittently hitting SQL error 1205 (deadlock victim).

## Prompt

Investigate the attached deadlock graph (XML from the system_health extended event session, a captured trace, or `sys.dm_tran_locks`/`sys.dm_os_waiting_tasks` output if the deadlock is reproducible live) and identify the root cause, not just which session was chosen as the victim.

From the deadlock graph, extract: the two (or more) sessions involved, the exact statements each was running, the resources each held and each was waiting on (identify whether they're key, page, RID, or object locks, and which index each resource belongs to), and the lock mode mismatch that created the cycle (e.g., one session holding an X lock while requesting an S/U lock the other already holds, or a classic bookmark-lookup deadlock where two sessions acquire locks on a clustered index and a nonclustered index in opposite order). Note the isolation level each session was running under, since it changes what locks are taken (e.g., SERIALIZABLE range locks vs. READ COMMITTED).

Propose a fix appropriate to the actual cause rather than a generic one: if it's a lock-ordering problem, recommend changing one code path so both access tables/rows in the same order; if it's a missing index forcing a scan that takes broader locks than necessary, propose the specific index; if it's READ COMMITTED SNAPSHOT ISOLATION (RCSI) being off and causing reader/writer blocking that escalates into a deadlock, evaluate whether enabling RCSI is appropriate for this database's workload (note the tempdb version-store cost); if it's a long-running transaction holding locks while doing unrelated work (e.g., an external call inside the transaction), recommend shrinking the transaction scope.

Include the exact `CREATE INDEX`, isolation-level change, or code reordering you're proposing, and explain how it breaks the specific cycle seen in the graph. Do not attempt to reproduce the deadlock by running load against a production database. Propose the fix and a safe reproduction/validation plan (e.g., a non-prod load test or a controlled two-session repro script), and get approval before applying anything to production.
