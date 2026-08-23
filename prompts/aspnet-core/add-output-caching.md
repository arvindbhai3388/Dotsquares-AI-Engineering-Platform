# Add Output Caching to a Read-Heavy GET Endpoint

**Category:** ASP.NET Core
**Use when:** a GET endpoint serves frequent requests for low-volatility data.

## Prompt

Analyze the GET endpoint I specify: how volatile its underlying data actually is (how often it changes, and whether staleness of a few seconds/minutes is acceptable to the business), whether any per-user or per-tenant variation exists in the response (which affects cache key design), and whether `Microsoft.AspNetCore.OutputCaching` is already configured anywhere else in Program.cs whose conventions I should match.

Propose the caching design before implementing: the cache duration, the varying-by dimensions (query string parameters, specific headers, authenticated user/tenant if the response differs per caller — get this wrong and you leak one user's data to another), whether this needs a named policy (`AddPolicy`) for reuse across similar endpoints, and the invalidation strategy — how the cache gets busted when the underlying data changes (tag-based eviction via `IOutputCacheStore.EvictByTagAsync`, a short TTL as the only mechanism, or both). Flag explicitly if the endpoint currently returns per-user data, since output caching by default is not safe for that without a correct `VaryByValue`/cache key strategy.

Once approved, implement:
- Register output caching via `AddOutputCache` with a named policy expressing the TTL, vary-by rules, and cache tags.
- Apply `.CacheOutput("policyName")` to the specific endpoint(s), not globally, unless a global default was explicitly requested.
- Tag cache entries so the write path (the corresponding POST/PUT/DELETE that mutates this data) can evict the specific tag rather than waiting out the TTL or flushing the entire cache.
- Ensure authenticated/per-user responses are never cached under a shared key — vary by the user identifier or exclude authenticated requests from caching entirely if per-user variation exists.
- Confirm this doesn't cache stale authorization state (a since-revoked token still getting a cached "allowed" response).

Write or update tests confirming: repeated requests within the TTL return the cached response (verify via a call counter/mock on the underlying data source), a request after mutation returns fresh data once the tag is evicted, and per-user isolation holds if applicable. Confirm with me before applying caching to any endpoint returning sensitive or per-user data.
