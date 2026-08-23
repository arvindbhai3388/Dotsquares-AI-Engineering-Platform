# Add Liveness/Readiness Health Checks

**Category:** ASP.NET Core
**Use when:** preparing a service for container orchestration or uptime monitoring.

## Prompt

Analyze this service's dependencies: database connection(s), downstream HTTP APIs, cache (Redis/in-memory/distributed), message queues, or any other external system it relies on to serve requests correctly. Check whether `Microsoft.Extensions.Diagnostics.HealthChecks` or any third-party health check packages (e.g., `AspNetCore.HealthChecks.SqlServer`) are already referenced, and whether any health endpoints already exist that this work should extend rather than duplicate.

Propose the health check design before implementing: a liveness endpoint (process is up, no dependency checks — safe for aggressive orchestrator restarts) versus a readiness endpoint (checks dependencies, used to gate traffic routing), which specific checks belong in readiness versus which are too expensive/noisy to run on every probe, timeout values for each check so a slow dependency can't hang the probe indefinitely, and the route paths and expected response format (plain 200/503 versus a JSON status payload) matching how the orchestrator or monitoring tool consuming this will parse it.

Once approved, implement:
- Register checks via `AddHealthChecks()` with named tags (e.g., `"live"`, `"ready"`) and map them to separate endpoints via `MapHealthChecks` with a `HealthCheckOptions.Predicate` filtering by tag.
- Give each custom `IHealthCheck` implementation a bounded timeout and make sure it never throws unhandled — catch and return `HealthCheckResult.Unhealthy` with a redacted (no connection strings/secrets) description instead.
- Do not put expensive or write-triggering operations in a check that runs frequently.
- Make sure checks respect the incoming `CancellationToken` so a slow probe doesn't leak background work.

Write or update tests for each custom `IHealthCheck` covering the healthy, degraded, and unhealthy paths, and an integration test asserting the endpoints return the correct status codes. Confirm with me which checks are liveness versus readiness before wiring them, since getting this wrong can cause orchestrator restart loops.
