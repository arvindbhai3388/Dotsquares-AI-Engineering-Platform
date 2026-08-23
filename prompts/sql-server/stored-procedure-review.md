# Review a Stored Procedure for Correctness, Injection, Transactions, and Performance

**Category:** SQL Server
**Use when:** A stored procedure is being modified, or is suspected of causing production issues.

## Prompt

Review the attached stored procedure end to end and report findings under four headings: correctness, SQL injection risk, transaction handling, and performance. Do not just restate what the procedure does — evaluate it against these specific failure modes.

Correctness: check for NULL-handling bugs (`= NULL` instead of `IS NULL`, `IN` lists containing NULL, `COUNT(column)` vs `COUNT(*)` mismatches), off-by-one or boundary errors in date/range predicates, and whether the procedure's output/side effects match its declared contract (e.g., a "Get" procedure that also mutates data). Check parameter defaults and optional-parameter handling for unintended matches (e.g., `WHERE Col = @Param OR @Param IS NULL` causing unexpected full scans or unexpected rows).

SQL injection: flag any dynamic SQL built by concatenation instead of `sp_executesql` with parameters, and any place a caller-supplied value is used to build an identifier (table/column name) without validation via an allow-list or `QUOTENAME()`.

Transaction handling: verify explicit transactions have a matching `COMMIT`/`ROLLBACK` on every code path, including error paths — check for `TRY/CATCH` with `XACT_STATE()` checks, `SET XACT_ABORT ON` where appropriate, and whether the procedure holds a transaction open across a slow or external call (which would extend lock duration and increase blocking risk). Note the isolation level in effect if not the connection default, and whether it's appropriate for what the procedure reads/writes.

Performance: check for RBAR (row-by-row) loops (`WHILE`/cursors) that could be set-based, unnecessary `SELECT *`, missing predicates causing large intermediate result sets, parameter-sniffing risk (the same procedure called with wildly different-cardinality parameters), and whether the procedure is a candidate for `OPTION (RECOMPILE)` or local variables to force a specific plan strategy. Use the execution plan if provided to substantiate any performance finding rather than guessing.

Rank findings by severity (correctness/security first, then performance), and for each propose the smallest fix rather than a rewrite. Do not modify or execute the procedure yourself — present findings and proposed changes, and wait for approval before implementing or running anything against a production or production-like database.
