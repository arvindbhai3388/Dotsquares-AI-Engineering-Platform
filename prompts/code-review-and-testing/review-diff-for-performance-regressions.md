# Review a Diff for Performance Regressions

**Category:** Code Review & Testing
**Use when:** A change touches a hot path or previously performance-tuned code.

## Prompt

Review the diff I specify exclusively for performance regressions -- assume correctness has already been reviewed separately, and focus this pass on whether the change makes a hot path slower, more memory-hungry, or less scalable than before.

Check specifically for:

1. **N+1 query patterns** -- a loop that issues a database call (via EF6/`DbContext`, this project's existing ADO.NET helper, or raw ADO.NET) per iteration instead of a single batched query; a new navigation-property access inside a loop that triggers lazy-loading per item instead of being eagerly included/joined up front.
2. **Unnecessary allocations** -- new allocations introduced inside a loop or a frequently-called method where a value could be hoisted, reused, or pooled; unnecessary LINQ chains that allocate intermediate collections (`.ToList()` followed immediately by another `.ToList()` or `.Select().ToList().Where()`) where a single pass would do; string concatenation in a loop instead of `StringBuilder` for a non-trivial number of iterations.
3. **Blocking calls in async code** -- `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` newly introduced on a `Task`, which can cause thread-pool starvation under load even though it "works" in a quick local test.
4. **Missing or broken caching** -- a change that removes or bypasses an existing cache lookup, invalidates a cache too aggressively (causing constant cache misses), or introduces a cache without an eviction/expiry strategy (unbounded growth).
5. **Synchronous I/O in a request path** -- new file, network, or database calls added to a request-handling path without async, or added to a path called far more frequently than the original design accounted for (e.g., a validation check that now calls out to an external service on every request instead of once per session).
6. **Serialization/payload size growth** -- a response DTO gaining fields that meaningfully increase payload size for a high-traffic endpoint, or a change that now serializes a full entity graph instead of a projection.

For each finding, explain the concrete performance impact in context (rows processed, expected call frequency, request volume if known) rather than a generic "this could be slow," since not everything flagged needs to be fixed -- a rarely-called admin endpoint doing an N+1 query is a very different priority than a checkout-path one. Rank findings by actual impact given the code's real usage pattern, and propose the smallest fix for each rather than a broader performance rewrite. If the diff includes a previously tuned section (comments referencing a past performance fix, benchmarks, or profiling notes), call out explicitly whether this change reverts or weakens that prior optimization.
