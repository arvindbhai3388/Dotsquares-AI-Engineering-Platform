# Add Rate Limiting to an Endpoint or API Surface

**Category:** ASP.NET Core
**Use when:** an endpoint is vulnerable to abuse or excessive load from a client.

## Prompt

Analyze the endpoint(s) or API surface I specify: current traffic patterns if known, whether this is a public or authenticated endpoint, what identifies a "client" for rate-limiting purposes (IP address, API key, authenticated user ID, tenant ID), and whether `Microsoft.AspNetCore.RateLimiting` is already configured anywhere in Program.cs.

Propose a rate-limiting design before implementing: which limiter algorithm fits the traffic shape (fixed window, sliding window, token bucket for bursty-but-bounded traffic, or concurrency limiter for expensive operations), the partition key (per-IP, per-API-key, per-user), the limit and window values (ask me for the actual numbers rather than guessing production thresholds), and what should happen when the limit is exceeded — a 429 with `Retry-After`, and whether it should be logged or fed into an alert/monitoring path.

Once I approve, implement it:
- Register the limiter via `AddRateLimiter` in Program.cs with a named policy, and apply it to the specific endpoint/group via `.RequireRateLimiting(...)` rather than globally, unless a global limiter was explicitly requested.
- Set `OnRejected` to return a ProblemDetails-shaped 429 response consistent with the rest of the API's error format, including `Retry-After` where the algorithm supports it.
- Make sure the partition key can't be trivially spoofed (e.g., don't key solely on a client-supplied header without validation; prefer `HttpContext.Connection.RemoteIpAddress` behind a trusted proxy, or the authenticated user's claim, configured correctly for any reverse proxy/`ForwardedHeaders` setup already in place).
- Avoid introducing state that breaks horizontal scaling (in-memory limiters are per-instance; call out to me if a distributed limiter/backing store is needed instead).

Write or update tests covering: requests under the limit succeed, requests over the limit return 429 with the correct headers, and the limiter resets after the window. Confirm with me before changing limits on any endpoint already in production use.
