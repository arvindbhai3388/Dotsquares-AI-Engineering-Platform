# Add a Row-Level Security Policy for Tenant/User Isolation

**Category:** SQL Server
**Use when:** A multi-tenant table currently relies only on application-layer filtering (a `WHERE TenantId = @TenantId` that every query must remember to add) instead of database-enforced isolation.

## Prompt

Design and propose a SQL Server Row-Level Security (RLS) policy for the attached table so tenant/user isolation is enforced at the database layer instead of depending on every query remembering an application-level filter. Start by identifying the tenant/owner column on the table (or the join path to it if it's not directly on the table) and how the current session establishes tenant identity (e.g., `SESSION_CONTEXT`, a custom `CONTEXT_INFO`, or an application-set variable) — do not assume `SESSION_CONTEXT` is already wired up; check for it explicitly, and if it isn't, include the connection-time `sp_set_session_context` call needed as part of the proposal.

Write an inline table-valued predicate function (`CREATE FUNCTION ... RETURNS TABLE ... WITH SCHEMABINDING AS RETURN SELECT 1 AS result WHERE ...`) that compares the row's tenant column against the session's tenant context, keeping the function simple and sargable so it doesn't itself become a scan bottleneck (avoid non-deterministic or expensive logic inside the predicate, since it's evaluated per row). Then create the `SECURITY POLICY` applying this function as both a `FILTER PREDICATE` (for SELECT/UPDATE/DELETE visibility) and, if inserts must also be constrained to the caller's own tenant, a `BLOCK PREDICATE` on INSERT/UPDATE.

Call out the specific edge cases: RLS predicates apply to ad hoc queries and reporting/BI tools connecting with the same login, which can silently change what "SELECT * FROM Table" returns for existing code — audit for any account (service accounts, admin tooling, the app's own connection pool) that legitimately needs cross-tenant access, and use `EXECUTE AS`/a policy `WITH (STATE = ON)` exemption or a separate elevated role deliberately rather than accidentally breaking it. Verify the predicate function doesn't defeat index usage (check the execution plan of a representative query post-policy).

Do not enable the policy against a production database yourself. Present the function, policy DDL, and session-context wiring change, list every access path that needs to be tested (app user, reporting account, migrations/jobs), and get explicit approval before applying anything outside a dev/test environment.
