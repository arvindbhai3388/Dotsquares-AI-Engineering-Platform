# Add a Code Coverage Threshold Gate

**Category:** Code Review & Testing
**Use when:** A team wants an objective minimum test-coverage bar without over-relying on the metric.

## Prompt

Add a code coverage threshold gate to the test project/CI pipeline I specify, using the coverage tooling already available to the project (e.g., `coverlet.collector`/`coverlet.msbuild` with `dotnet test --collect:"XPlat Code Coverage"`, feeding a report tool like ReportGenerator) -- do not introduce a new coverage tool if one is already wired up.

Configure the gate with:

1. A line-coverage (and, if the tooling supports it, branch-coverage) minimum threshold set per project rather than solution-wide, since a single global number hides which specific project regressed. Propose a starting threshold based on the project's *current* measured coverage (e.g., current minus a small buffer, or current rounded down) rather than an arbitrary round number that might immediately fail the build if today's coverage is lower.
2. The build/CI step failing when coverage drops below the threshold, with the failure message stating the actual vs. required percentage and which project failed.
3. Sensible exclusions -- auto-generated code (EDMX-generated context classes, designer files), DTOs/POCOs with no logic, and `Program.cs`/startup wiring that's better covered by integration tests than unit tests -- excluded explicitly via attribute or config, not by gaming the measurement.

Critically, alongside the gate, write a short explanation (as a comment in the config or in the PR description) of what coverage numbers do and do not guarantee, so the team doesn't develop false confidence from a passing gate:

- A covered line only proves it *executed* during a test run -- it says nothing about whether any assertion would catch a bug there. A test with no assertions, or only a trivial `Assert.IsNotNull`, can produce 100% coverage on completely unverified behavior.
- High coverage on trivial code (getters/setters, simple mapping) inflates the number without adding real safety, while a single complex, high-risk method left uncovered may matter far more than the aggregate percentage suggests.
- The gate should be treated as a floor that stops obvious regressions (a large new chunk of business logic shipped with zero tests), not a target to maximize -- do not encourage writing assertion-free tests purely to raise the number.

Recommend pairing the coverage gate with an occasional mutation testing pass (see the mutation-testing prompt in this library) for modules where coverage alone isn't sufficient assurance. After wiring the gate, run the build once to confirm it correctly passes at current coverage and correctly fails when you temporarily drop the threshold above current coverage, proving the gate actually functions.
