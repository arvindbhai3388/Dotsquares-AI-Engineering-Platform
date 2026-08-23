# Add Architecture Tests with NetArchTest

**Category:** Code Review & Testing
**Use when:** Layering violations keep creeping in without anyone noticing until much later.

## Prompt

Add architecture/dependency rule tests using NetArchTest.Rules for the solution/project I specify, to catch layering violations automatically instead of relying on reviewers to notice them by eye in every PR.

First, confirm the actual intended layering by reading the project structure and any existing architecture documentation rather than assuming a textbook layering that may not match this codebase's real design (e.g., confirm whether "Domain must not reference Infrastructure" is genuinely the intended rule here, or whether the real constraint is something more specific to how this solution is organized, such as plugin projects not referencing each other, or the web project not directly referencing ADO.NET types that belong in a data-access layer).

Write the rules as a small xUnit (or the project's existing test framework) test class, with one test method per rule, each asserting on a `Types.InAssembly(...)` / `.That()...` / `.Should()...` NetArchTest chain and calling `.GetResult().IsSuccessful`. Typical rules to consider, adapted to what's actually true of the target codebase:

- A given layer/namespace must not reference another specific layer/namespace (e.g., `*.Domain` must not depend on `*.Infrastructure` or `*.Data`).
- Classes in a given namespace must implement/inherit from an expected base type or interface (e.g., all classes under `*.Services` implement an `I*Service` interface).
- No class outside a designated set may reference a specific external library type directly (e.g., only the persistence layer may reference `System.Data.SqlClient`/`Microsoft.Data.SqlClient` types).
- Naming/visibility conventions worth enforcing mechanically (e.g., classes implementing a plugin contract must be `public` and end in a specific suffix, if that convention already exists informally).

When a rule test fails, make the failure message list the specific offending types (`result.FailingTypeNames`), not just "rule failed," so a developer can immediately see what to fix.

Do not invent rules the codebase doesn't actually follow today just to have coverage -- if the current code already violates a rule that should exist going forward, tell me before adding a red test; either fix the current violations first (as a separate, explicit change) or scope the rule to exclude the known legacy violations with a clear TODO/tracking note, rather than silently weakening the rule to pass. Run the tests after adding them and report which rules pass against the current codebase as-is.
