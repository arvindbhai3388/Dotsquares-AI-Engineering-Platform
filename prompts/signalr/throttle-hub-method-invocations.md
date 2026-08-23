# Throttle Hub Method Invocations

**Category:** SignalR
**Use when:** a chatty client action (typing indicators, cursor position, live search-as-you-type) risks overwhelming the server.

## Prompt

Add throttling/rate limiting to the specified hub method (<name it, e.g., a typing-indicator or cursor-position update method>) that clients could otherwise call excessively, causing unnecessary server load or downstream broadcast storms to other connected clients. Before implementing, check whether ASP.NET Core's built-in rate limiting middleware applies to SignalR hub invocations in this app's version/configuration (it generally does not apply per-method to hub methods the way it does to HTTP endpoints), and confirm no existing throttling helper already exists in the codebase that should be reused instead of writing a new one.

Design and propose for approval:
- A per-connection (or per-user, if multiple connections per user should share a budget) throttle using a lightweight mechanism appropriate to the load -- e.g., a token bucket or simple last-invocation-timestamp check stored in a ConcurrentDictionary<string, ...> keyed by Context.ConnectionId, or ASP.NET Core's RateLimiter primitives (SlidingWindowLimiter/TokenBucketRateLimiter) wrapped for hub use if the team wants a standard implementation over a hand-rolled one.
- What happens when a client exceeds the limit: silently drop the call (appropriate for pure UI signals like typing indicators, where a missed update is harmless and self-corrects on the next one), versus throwing a HubException back to the caller (appropriate where the client needs to know its action didn't take effect) -- pick based on the semantics of this specific method and state the choice explicitly.
- Server-side broadcast fan-out: even if the throttle limits how often one client can send, also consider whether the resulting broadcast to a large group (e.g., typing indicator to a 500-person room) needs its own coalescing/debounce (e.g., only rebroadcast if the state actually changed) independent of the per-sender throttle.
- If the app scales out across multiple instances (backplane or Azure SignalR Service), a per-instance in-memory throttle undercounts a client that could theoretically reconnect to a different instance -- decide whether this matters for this specific method's risk profile (usually acceptable for soft signals like typing indicators) or whether a shared store is warranted, and document the decision rather than silently accepting a gap.
- Avoid throttling logic that blocks the hub's method-invocation pipeline for other clients -- keep the check fast and non-blocking (no synchronous locks that serialize unrelated clients' calls).

After approval, implement it, then write a test that invokes the method faster than the configured limit and asserts the excess calls are handled per the chosen policy (dropped or rejected), plus a test confirming calls within the limit are unaffected.
