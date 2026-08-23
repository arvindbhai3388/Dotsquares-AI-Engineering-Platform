# Draft a Load Test Scenario

**Category:** Code Review & Testing
**Use when:** A new endpoint needs performance validation before go-live.

## Prompt

Draft a load test scenario for the endpoint I specify, using k6 (preferred for HTTP APIs, JavaScript-based scripting) or NBomber (preferred if we want the scenario written in C# alongside the rest of the codebase) -- pick whichever the team already has tooling/CI support for, and ask if neither is established yet.

Before writing the script, gather the inputs needed to make the scenario realistic rather than arbitrary:

- Expected real-world traffic pattern: peak requests/second, typical request/response payload size, and whether traffic is bursty or steady.
- The request's actual shape, including realistic variation in inputs (not the same exact request replayed thousands of times, which can hide caching-related false confidence and won't exercise real data distribution).
- Any auth required (token acquisition step before the load stage, since token issuance itself shouldn't be load-tested as part of the same loop unless that's the point).
- Downstream dependencies the endpoint calls, and whether those need to be pointed at a test/staging instance sized appropriately -- a load test that overwhelms a shared downstream dependency other services rely on is a real production risk, so confirm the target environment first.

Structure the script with:

1. **Ramp-up** -- start well below expected peak and increase gradually (e.g., stepped stages over several minutes) rather than an instant spike, to see how the system behaves as load builds, matching realistic traffic growth rather than a step function.
2. **Think time** -- realistic pauses between a virtual user's requests if the scenario simulates a user session with multiple calls, rather than firing requests back-to-back with no gap, which would test a pattern no real user produces.
3. **Sustained peak** -- hold at target peak load for long enough to reveal issues that only appear under sustained pressure (connection pool exhaustion, memory growth, GC pressure), not just an instantaneous spike.
4. **Ramp-down** and a clear thresholds/pass-fail definition -- explicit SLOs such as p95 latency under N ms and error rate under X%, defined before the run, not decided after looking at results.

Report results against those pre-defined thresholds, flag the first bottleneck observed (with supporting metrics: latency percentiles, error rate, resource utilization if available), and do not run the load test against a shared/production environment without my explicit confirmation of the target environment first.
