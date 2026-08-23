# Add a Compiled Query for a High-Throughput Hot Path

**Category:** Entity Framework Core
**Use when:** Profiling shows query compilation cost on a very high-throughput query.

## Prompt

Profiling has shown that [name the query/method] spends measurable time on LINQ-to-SQL query compilation rather than execution, because it runs at very high frequency (e.g., a hot API path called thousands of times per second). I want to evaluate and, if justified, add an `EF.CompileQuery`/`EF.CompileAsyncQuery` for it. Use analyze -> propose -> approve -> implement -> test -> review, and confirm the plan with me before implementing.

Analyze:
1. Confirm this is actually a compilation-cost problem and not an execution-cost problem -- EF Core already caches compiled query plans internally by default for parameterized queries with the same shape, so compiled queries mainly help when profiling (e.g., dotnet-trace, MiniProfiler, or EF's own logging) shows meaningful time in `IQueryCompiler`/expression tree processing, not just DB round-trip time.
2. Identify the exact query shape: compiled queries require a fixed shape with typed parameters (no dynamically-built `IQueryable` with conditional `.Where()` chains) -- flag if the current query is built dynamically and can't be compiled as-is without restructuring.
3. Check whether the query is invoked with a fresh DbContext instance each time (compiled queries require the context type to match exactly) or through DI in a way compatible with a static compiled delegate.

Propose:
- Show the exact `private static readonly Func<MyDbContext, TKey, IAsyncEnumerable<TResult>> _compiledQuery = EF.CompileAsyncQuery(...)` declaration and where it should live (a static field near the DbContext or repository, not recreated per call).
- Explain the constraint that compiled queries bypass some dynamic LINQ conveniences (no dynamic predicate composition) and must be kept in sync manually if the entity model changes.
- If profiling doesn't clearly show compilation overhead as the bottleneck, recommend against adding this complexity and suggest addressing indexing, projection, or caching instead.

Wait for approval before implementing.

Implement the compiled query, replacing the hot-path call site only, without touching other call sites of the same logical query if they aren't on the hot path.

Test: verify functional correctness is unchanged, and if possible, benchmark before/after compilation overhead (e.g., BenchmarkDotNet) to confirm the change is worth the added complexity.

Review: confirm the compiled query is kept isolated and documented so future schema changes aren't missed.
