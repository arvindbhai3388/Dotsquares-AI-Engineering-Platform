# Add Retry-with-Backoff for Power BI API Throttling

**Category:** Power BI
**Use when:** API calls to Power BI intermittently fail under load with throttling errors.

## Prompt

Add retry-with-backoff handling for Power BI REST API throttling (HTTP 429 "Too Many Requests") across the .NET client code that calls Power BI (embed-token generation, dataset refresh triggers/polling, export-to-file, admin API calls, etc.). Before implementing, locate every place in this codebase that calls the Power BI REST API or the Power BI .NET SDK client, since throttling handling needs to be applied consistently rather than patched into one call site while others remain fragile -- propose a single shared solution (a delegating handler, a Polly policy, or a wrapper client) rather than duplicating retry logic per call site.

Requirements:
- Check whether this app already has a retry/resilience library in use (Polly is common and is already a dependency in some .NET 8 services in similar solutions) -- reuse it if present rather than hand-rolling a retry loop; if none exists, propose adding one and get approval before introducing the new dependency, per this platform's dependency-approval discipline.
- On a 429 response, respect the `Retry-After` header if Power BI includes one, rather than using a fixed or purely exponential delay that ignores the server's explicit guidance.
- Use exponential backoff with jitter as the fallback when no `Retry-After` header is present, with a sensible max retry count (e.g. 3-5 attempts) and a max total wait time, so a persistently throttled/failing call fails fast eventually rather than retrying indefinitely and hanging a request.
- Distinguish 429 (throttling, retry) from other error codes that should NOT be retried: 401/403 (auth failure -- retrying with the same expired/invalid credential just wastes time and can trigger AAD lockout policies), 404 (resource genuinely doesn't exist), and 400 (bad request -- a malformed request will fail identically on retry).
- Log throttling events (count, endpoint, backoff duration) at a level that lets the team notice if throttling is becoming frequent (a sign the app needs a higher-tier capacity or better request batching), but avoid log spam on routine single-retry recoveries.
- Ensure the retry logic composes correctly with existing per-call timeouts and cancellation tokens -- a retry loop must still respect a caller's `CancellationToken` and not silently extend an operation past its intended timeout.

Write unit tests simulating a 429-then-success sequence and a 429-exhausted-retries sequence using a mocked `HttpMessageHandler`, confirming both the backoff behavior and that non-retryable status codes (401, 404, 400) fail immediately without retrying.
