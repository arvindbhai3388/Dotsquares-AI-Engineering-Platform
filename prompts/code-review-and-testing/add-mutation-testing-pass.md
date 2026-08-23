# Run a Mutation Testing Pass with Stryker.NET

**Category:** Code Review & Testing
**Use when:** Code coverage is high but confidence in the test suite's actual rigor is low.

## Prompt

Set up and run a mutation testing pass with Stryker.NET against the module/project I specify, then interpret the results to find tests that pass but don't actually verify anything meaningful.

First explain briefly why this matters: code coverage only proves a line executed during a test run, not that any assertion would catch a bug on that line. Mutation testing rewrites the production code in small ways (a mutant) -- flipping a `>` to `>=`, changing a `+` to `-`, removing a null check, negating a boolean condition, changing a returned constant -- and reruns the test suite against each mutant. A mutant that "survives" (tests still pass despite the code being wrong) reveals a gap the coverage number was hiding.

Steps:

1. Install/configure Stryker.NET (`dotnet-stryker` tool) scoped to the target project, using a `stryker-config.json` that limits the mutation run to the module in question rather than the whole solution, to keep run time reasonable.
2. Run the mutation test pass and capture the report (mutation score, survived vs. killed vs. no-coverage mutants).
3. For each **survived** mutant, read the mutated line and the tests that exercise it, and classify why it survived: (a) no test actually asserts on the affected behavior, (b) the assertion is too loose (e.g., checking `IsNotNull` instead of the actual value), (c) the mutated branch is genuinely unreachable/dead code, or (d) the mutant is equivalent (behaviorally identical, e.g., mutating unreachable log-only code) and can be legitimately ignored.
4. For categories (a) and (b), propose the specific test change or new test case needed to kill the mutant -- a tightened assertion or a new test targeting that branch/condition.
5. Report a mutation score summary and a short list of the highest-value fixes (mutants in business-critical logic first, cosmetic/logging-adjacent mutants last).

Do not treat "kill every mutant" as the goal in itself -- flag genuinely equivalent mutants rather than writing contrived tests to kill them, since that adds maintenance cost without real value. Present findings and proposed test additions for approval before implementing them, and after implementing, rerun the affected mutants (or the full pass if scope is small) to confirm the mutation score actually improved.
