# Review a MERGE Statement for Concurrency Safety

**Category:** SQL Server
**Use when:** A MERGE-based upsert is behaving unexpectedly under concurrent execution (duplicate inserts, unexpected updates, or intermittent errors).

## Prompt

Review the attached `MERGE` statement for the well-documented correctness and concurrency pitfalls that make MERGE riskier than it first appears, and propose a fix appropriate to the specific issue found rather than a blanket rewrite.

Check for the race condition that causes duplicate-key errors or duplicate rows under concurrent execution: MERGE's WHEN MATCHED/WHEN NOT MATCHED evaluation is not atomic with respect to another concurrent MERGE or INSERT against the same key under READ COMMITTED (the default), so two sessions can both evaluate "not matched" for the same key and both attempt an insert. Confirm whether this table has a unique constraint/index on the natural key that would at least surface the race as an error rather than silent duplication, and if concurrent upserts against the same key are a realistic scenario, recommend either wrapping the MERGE in a transaction with an appropriate isolation level (SERIALIZABLE or using `HOLDLOCK`/`UPDLOCK` hints on the target table's join to force a range lock that blocks the second session until the first commits) or, if the workload allows, an `INSERT ... WHERE NOT EXISTS` pattern with a unique constraint as the actual duplicate-prevention mechanism and `TRY/CATCH` around the constraint-violation error instead of relying on MERGE's own matching logic.

Check for trigger-interaction issues: MERGE fires triggers per action clause (INSERT/UPDATE/DELETE), and `INSERT ... EXECUTE` or nested trigger logic on the target table can behave differently than expected compared to separate statements — verify `inserted`/`deleted` virtual tables inside any trigger on this table reflect what the trigger logic assumes when invoked via MERGE specifically, not just via a plain INSERT/UPDATE.

Check for the WHEN MATCHED ambiguity bug: if the join in the USING clause can match a source row to more than one target row (or vice versa), MERGE raises "The MERGE statement attempted to UPDATE or DELETE the same row more than once" — confirm the join predicate is actually unique on the target side. Also verify `$action` isn't relied upon incorrectly in an OUTPUT clause if row counts matter downstream.

Present each finding with a concrete concurrent-execution scenario that would trigger it, and the proposed fix (locking hint, isolation level, or restructuring to separate INSERT/UPDATE statements). Do not apply the fix or run load tests against production yourself — propose it and get approval before testing beyond a non-production environment.
