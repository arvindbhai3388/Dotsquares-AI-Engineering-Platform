# Add DbContext Pooling for a High-Throughput API

**Category:** Entity Framework Core
**Use when:** DbContext construction overhead is measurable under load.

## Prompt

Under load, [name the API/service] is spending measurable time constructing `DbContext` instances per request (confirmed via profiling or an observed CPU/latency pattern that improves when concurrency is lower). I want to evaluate and, if justified, switch from `AddDbContext` to `AddDbContextPool` to reuse context instances. Follow analyze -> propose -> approve -> implement -> test -> review; confirm the plan before changing DI registration.

Analyze:
1. Confirm the actual bottleneck is DbContext construction/service-provider resolution overhead (common in very high-throughput scenarios with many small, simple DbContext instances), not query execution time -- pooling doesn't help query performance at all.
2. Audit the DbContext class and everything injected into it via its constructor for ANY per-request or per-user state: fields set in the constructor from `IHttpContextAccessor`, a "current user" service, a tenant ID, or any mutable field set later via a setter method and expected to reset per use. `AddDbContextPool` requires the context to be safely reusable across requests -- any such state will leak across requests if not explicitly reset.
3. Check for any `OnConfiguring` override doing per-request configuration (e.g., building a connection string dynamically per tenant) -- pooled contexts are configured once per pooled instance's lifetime, so dynamic per-request configuration inside the context itself is incompatible with pooling as-is.

Propose:
- If the DbContext is "clean" (constructor takes only `DbContextOptions<T>` and static/singleton dependencies): show the exact `services.AddDbContextPool<MyDbContext>(options => ..., poolSize: <N>)` change and recommend a pool size based on expected peak concurrency, not an arbitrarily large number (idle pooled instances still hold memory).
- If per-request state exists (tenant ID, current user), propose overriding `DbContext.ResetState()` (EF Core's pooling reset hook) to clear/reset that state on return to the pool, or propose keeping such state OUT of the DbContext entirely (injected per-call into repository methods instead) so the context itself has zero per-request state -- explain the tradeoff between the two.
- Flag that `ChangeTracker.Clear()` is called automatically by pooling on return, but any custom fields are the developer's responsibility to reset.

Wait for approval, then implement the DI change and any required `ResetState()` override or refactor to remove per-request state from the context.

Test: write a test simulating two "requests" reusing the same pooled context sequentially, asserting no state (tracked entities, tenant ID, current user) leaks from the first into the second.

Review: confirm no per-request field was missed in the reset logic -- this is the single highest-risk mistake with pooling.
