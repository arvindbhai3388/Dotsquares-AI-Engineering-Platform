# Audit the Test Suite for Flaky Tests

**Category:** Code Review & Testing
**Use when:** CI test runs fail intermittently without code changes.

## Prompt

Investigate the intermittently failing test(s) I specify (or, if the whole suite is suspect, help me identify which ones are flaky first) and fix the actual root cause -- not by adding retries or increasing timeouts as a first resort, since that hides the underlying problem rather than fixing it.

Diagnostic steps:

1. Reproduce the flakiness locally by running the suspect test repeatedly in a loop (e.g., `dotnet test --filter` targeting just that test, run N times), and separately by running the full suite with tests in parallel and in randomized order if the runner supports it, since order-dependent flakiness often only appears under parallel/randomized execution, not a single sequential run.
2. Once reproduced, classify the root cause -- it is almost always one of:
   - **Shared mutable state** -- a static field, a shared in-memory collection, or a singleton service instance whose state leaks between tests because it isn't reset in setup/teardown, causing a test to pass or fail depending on what ran before it.
   - **Execution-order assumptions** -- a test that only passes because another specific test happens to run first and leaves the system in a state this test silently depends on (a database row, a cache entry, a static counter).
   - **Timing dependencies** -- a test asserting on `DateTime.Now`-derived values without controlling time, a race between an async operation and an assertion that runs before the operation completes (missing `await`, or asserting immediately after firing a background task), or a hardcoded `Thread.Sleep` duration that's sometimes not long enough under CI load.
   - **External dependencies** -- a test that hits a real network resource, a shared test database, or the file system in a way that collides with concurrent test runs (same file path, same DB row) or is subject to real network flakiness.
   - **Non-deterministic ordering assumptions on collections** -- asserting on the order of items from a `Dictionary`/`HashSet`/parallel query when no ordering is actually guaranteed.
3. Fix the specific root cause: isolate shared state per test (fresh instances in setup, no static mutation), remove order dependencies (each test creates its own required preconditions), replace real timing/sleeps with deterministic waits or injectable time providers, and isolate external dependencies via test doubles or per-test-scoped resources (unique keys/paths per test run).

Do not silently add `[Retry]`-style attributes, increase a timeout, or mark the test `[Ignore]`/`[Skip]` as the fix unless the root cause turns out to be a genuinely unavoidable external factor and I've explicitly agreed that's acceptable -- in that case, document why in a comment on the test. After the fix, rerun the test in the same repeated/parallel/randomized-order loop that reproduced the flakiness to confirm it's now reliably passing, and report how many consecutive runs you verified.
