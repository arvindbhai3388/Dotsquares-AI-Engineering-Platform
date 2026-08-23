# Production Readiness Checklist

> **Template usage:** Pre-deploy gate for a client project change, distinct from the
> per-diff `code-review-checklist.md`. Run this before a release/deploy that includes
> non-trivial changes (new feature, schema change, new integration, infra change). Not every
> item applies to every deploy — mark N/A with a reason rather than skipping silently.

## 1. Configuration & Secrets

- [ ] All secrets, connection strings, API keys, and tokens are externalized (environment
      variables, user-secrets, Key Vault/App Configuration, or equivalent) — none hardcoded
      or committed.
- [ ] Environment-specific configuration (Dev/Staging/Prod) is confirmed correct for the
      target environment — not copied from another environment without review.
- [ ] New configuration keys are documented (placeholder shape, not real values) so ops/other
      developers know what to set.
- [ ] Feature flags used to gate new behavior are set to the intended state for this release.

## 2. Database & Migrations

- [ ] Migrations have been tested against a copy of production-like data volume, not just an
      empty local database.
- [ ] Migration is backward-compatible with the currently deployed application version if
      there's any window where old code and new schema (or vice versa) coexist.
- [ ] Long-running migrations (large table alters, index builds) have been evaluated for
      lock/timeout impact and scheduled appropriately.
- [ ] A rollback script or documented rollback path exists for the migration.
- [ ] Seed/reference data changes, if any, are idempotent (safe to run twice).

## 3. Logging & Monitoring

- [ ] New code paths have appropriate logging at the right level (not everything at
      `Information`, not silent on failure).
- [ ] No secrets, tokens, connection strings, or unnecessary personal data are logged.
- [ ] Errors/exceptions in new code surface to existing monitoring/alerting (Application
      Insights, Serilog sinks, health checks) rather than being swallowed.
- [ ] New external dependencies (APIs, queues, third-party services) have timeout and
      retry/circuit-breaker behavior appropriate to their criticality.
- [ ] Health check endpoints, if the project has them, reflect the new dependency's
      availability if it's on a critical path.

## 4. Rollback Plan

- [ ] It's clear how to roll back the application deployment itself (previous artifact/image
      redeploy, feature flag off) independent of any database rollback.
- [ ] The rollback plan has been communicated to whoever is on call/deploying.
- [ ] Any irreversible action introduced by this release (data migration that deletes/
      transforms data, one-way schema change) is called out explicitly, with a mitigation if
      something goes wrong.

## 5. Performance & Load

- [ ] New queries have been checked for N+1 patterns and missing indexes on tables expected
      to be large or high-traffic.
- [ ] Any new endpoint/job that could be called at volume has been considered for rate
      limiting, pagination, or batching as appropriate.
- [ ] New background/scheduled work has bounded concurrency and won't starve other work
      (thread pool, DB connection pool, external API rate limits).
- [ ] If load/perf testing was warranted for this change, it was done and results are
      acceptable — or the decision to skip it is explicit and justified.

## 6. Security

- [ ] New endpoints/pages enforce the same authentication/authorization as equivalent
      existing ones.
- [ ] Dependencies added since the last release have been checked for known vulnerabilities
      (e.g. `dotnet list package --vulnerable`) where the project has that tooling.
- [ ] No new attack surface (file upload, deserialization, external redirect, new external
      call) was introduced without corresponding validation.
- [ ] TLS/HTTPS enforcement and CORS configuration remain correct for the target environment.

## 7. Backward Compatibility

- [ ] Existing API consumers, other services, or client apps calling into changed code will
      not break — verified against the actual contract, not assumed.
- [ ] Any breaking change has been communicated to affected teams/clients ahead of the
      release, with a migration path.

## 8. Documentation & Handoff

- [ ] Project documentation (`wiki/`-style docs, README, or in-repo `Notes/`) updated to
      reflect the change, if it affects how the system is operated or understood.
- [ ] Client-facing summary generated for new features/client-visible changes, if this
      platform's `client-summary` workflow applies to this project.
- [ ] Whoever is deploying/on-call has what they need: what changed, how to verify it worked,
      and how to roll back.
