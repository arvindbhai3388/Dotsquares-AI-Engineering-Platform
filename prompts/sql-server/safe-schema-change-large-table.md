# Plan a Safe Schema Change on a Large, Actively-Used Table

**Category:** SQL Server
**Use when:** A schema change (e.g., adding a NOT NULL column) risks a long blocking operation on a hot production table.

## Prompt

Plan a safe rollout for the attached schema change (e.g., adding a `NOT NULL` column, changing a column's type, adding a constraint) against a large, actively-used production table, using an expand/contract approach rather than a single blocking DDL statement. First determine whether the naive version of this change is actually blocking: adding a `NOT NULL` column without a default is always blocking and requires a table rewrite in older engines, but check the SQL Server version/edition in use — SQL Server 2012+ can add a `NOT NULL` column with a constant default as a fast, metadata-only operation (no table rewrite) as long as the default is a runtime constant, not a function; adding it with a non-constant default (e.g., `NEWID()`, a subquery) still requires a full table update. Verify which case applies here before assuming a workaround is needed.

Where a metadata-only fast default doesn't apply (changing an existing column's type, adding a computed/derived NOT NULL column, or backfilling from other data), design the change as an expand/contract sequence: (1) expand — add the new column as NULLable first, (2) backfill — populate it in small batches (e.g., `UPDATE TOP (n) ... WHERE NewCol IS NULL`, looping and pausing/committing between batches) to avoid one giant transaction holding locks and growing the log, (3) contract — once backfilled and verified, add the `NOT NULL` constraint (`WITH CHECK` vs. `WITH NOCHECK` decision stated explicitly, since `NOCHECK` skips validating existing rows) or switch application writes to the new column and drop the old one in a later, separate deployment so the change is backward-compatible with code still running the previous version during a rolling deployment.

Call out concurrency and rollback: for each phase, state the lock type/duration expected, recommend running backfills during low-traffic windows, and confirm the plan is reversible at each step (i.e., the table remains queryable by old and new application code simultaneously mid-migration, not just at the end). If the table is large enough that even a NULLable column add or index-related side effect needs an online operation, note edition-specific options (e.g., `ONLINE = ON` for supported operations).

Present the full phased plan with the exact DDL for each phase and the expected impact/duration per phase. Do not execute any phase against production yourself — propose the plan, recommend timing a rehearsal against a production-sized copy, and get explicit approval before each phase runs against production.
