# Configure Connection Resiliency with EnableRetryOnFailure

**Category:** Entity Framework Core
**Use when:** Transient DB connectivity errors (e.g., in Azure SQL) are causing intermittent failures.

## Prompt

We're seeing intermittent failures from [name the service/DbContext] that look like transient database connectivity issues (timeouts, connection resets, Azure SQL throttling/failover errors) rather than genuine application bugs. I want to configure EF Core's connection resiliency (`EnableRetryOnFailure`) correctly, including its interaction with explicit transactions. Follow analyze -> propose -> approve -> implement -> test -> review; confirm the plan before changing configuration.

Analyze:
1. Confirm the actual exception types/error codes being hit (e.g., `SqlException` with specific transient error numbers, or `TimeoutException`) to distinguish genuinely transient failures from a real bug being misdiagnosed as "flaky."
2. Search for any place in the codebase that already wraps DbContext operations in an explicit `BeginTransaction()`/`TransactionScope`, since `EnableRetryOnFailure` and manually-created transactions conflict: EF Core's execution strategy needs to control retries around the whole transaction, and an explicitly-opened transaction that isn't wrapped in `context.Database.CreateExecutionStrategy().ExecuteAsync(...)` will throw `InvalidOperationException` when retries are enabled.
3. Check current `DbContextOptionsBuilder` configuration for the provider (SQL Server via `UseSqlServer`, etc.) to see where retry configuration should be added.

Propose:
- Show the exact configuration: `options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null))`, with retry count/delay tuned to the operation's latency budget (don't set retries so high that a user-facing request hangs unacceptably long).
- For every call site with an explicit transaction, show the required refactor to `var strategy = context.Database.CreateExecutionStrategy(); await strategy.ExecuteAsync(async () => { using var tx = await context.Database.BeginTransactionAsync(); ... await tx.CommitAsync(); });` so the whole unit of work retries atomically instead of retrying a query mid-transaction.
- Flag idempotency: if any operation inside a retried block has non-transactional side effects (e.g., sending an email, calling an external API) that could double-fire on retry, propose moving that side effect outside the retryable unit of work or making it idempotent.
- Note that connection-level resiliency doesn't replace request-level circuit breaking if the outage is prolonged -- mention Polly only if the codebase already uses it elsewhere, rather than introducing a new dependency for this alone.

Wait for approval, then implement the configuration change and required transaction refactors.

Test: if feasible, simulate a transient failure (e.g., a fault-injection test double or a network-level test) to confirm retry behavior fires and eventually succeeds or fails cleanly after exhausting retries.

Review: confirm every explicit transaction in the affected DbContext's usage was updated to use the execution strategy, not just the one that triggered the investigation.
