# Add a Backplane for Multi-Instance SignalR

**Category:** SignalR
**Use when:** self-hosting SignalR across multiple instances or load-balanced servers without using Azure SignalR Service.

## Prompt

Add a backplane (Redis or SQL Server -- confirm which one fits the existing infrastructure before choosing) so SignalR messages are delivered correctly to all clients regardless of which app server instance they're connected to. Analyze the current deployment first: how many instances/servers run this app behind the load balancer, whether sticky sessions/ARR affinity are currently configured (they should NOT be required once a backplane is added, but confirm current state), and what shared infrastructure (existing Redis cache, SQL Server) is already available to reuse rather than provisioning something new. Propose the choice and configuration for my approval before implementing.

Cover these specifics:
- If Redis is available and preferred: add Microsoft.AspNetCore.SignalR.StackExchangeRedis and wire services.AddSignalR().AddStackExchangeRedis(connectionString, options => ...), sourcing the connection string from existing configuration (never hardcode it or have me paste it into chat).
- If SQL Server is the only shared infra: add Microsoft.AspNetCore.SignalR.SqlServer and wire services.AddSignalR().AddSqlServer(connectionString), noting this has higher latency than Redis and is appropriate only for lower-throughput scenarios -- flag this tradeoff explicitly if throughput requirements aren't already known.
- Confirm every Clients.Group/Clients.User/Clients.All broadcast still reaches clients connected to a different instance than the one that issued the broadcast -- this is the entire point of the backplane, so include a manual or automated test that starts two instances locally, connects a client to each, and verifies a broadcast from one instance's IHubContext reaches the client on the other.
- Audit for any in-memory, per-process state used for connection tracking, group bookkeeping, or presence (e.g., static dictionaries) that will now be inconsistent across instances -- this must move to the backplane-backed mechanisms (Groups.AddToGroupAsync, which is backplane-aware) or a genuinely shared store, not left as process-local state.
- Confirm load balancer configuration: sticky sessions are typically still recommended even with a backplane (to avoid unnecessary reconnects), so don't assume the backplane eliminates the need for session affinity entirely -- verify against the chosen backplane's documented guidance rather than assuming.
- Consider backplane failure/latency: what happens to message delivery if Redis/SQL becomes briefly unavailable -- does the app degrade gracefully or do broadcasts silently fail?

After approval, implement, then validate with a genuine multi-instance test (locally via two process instances or in staging) rather than a single-instance test that can't actually exercise the backplane, and report exactly what was verified.
