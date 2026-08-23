# Add Moq-Based Unit Tests for a Service

**Category:** Code Review & Testing
**Use when:** A service's correctness depends on how it calls its collaborators, not just its return value.

## Prompt

Write unit tests using Moq for the service class I specify, which has multiple injected dependencies. The goal is to verify both what the service returns and how it uses its collaborators, since some of this service's correctness lives in call sequencing and arguments passed downstream, not just the final return value.

For each dependency, decide deliberately between a mock (verify interactions) and a stub (just return canned data) based on what that test actually needs to prove -- don't default to mocking everything. Set up:

- `Mock<IDependency>.Setup(...)` for return values needed to drive the method under test through each code path (success, empty result, dependency throws).
- `Verify(...)` calls where the test's purpose is to confirm a collaborator was invoked -- check not just call count (`Times.Once`, `Times.Never`, `Times.Exactly(n)`) but also the actual arguments passed, using `It.Is<T>(x => ...)` predicates rather than `It.IsAny<T>()` wherever the argument value matters to correctness (e.g., confirming an audit log call received the correct user ID, or a repository save received the mutated entity, not the original).
- `Mock.Of<T>()` or a plain stub only where the dependency is incidental to the test and its exact usage doesn't need verifying -- keep tests readable by not over-specifying uninteresting collaborators.

Cover: the happy path where all dependencies succeed; a dependency throwing an exception and the service either propagating, wrapping, or handling it per its documented contract; a dependency returning null/empty/default and the service's handling of that; and, if the service short-circuits under some condition (e.g., cache hit, feature flag off), a test proving the downstream dependency is NOT called (`Times.Never`) in that case.

Keep tests independent -- construct a fresh set of mocks per test (via constructor/setup method, not shared static mocks) so verifying call counts in one test is never affected by another test's calls. Use Arrange-Act-Assert structure and name tests `MethodName_Scenario_ExpectedResult`. After writing the tests, run them and report actual pass/fail results. If a `Verify` fails because the service doesn't actually call the dependency the way the ticket/requirement implies it should, flag that as a potential real bug rather than loosening the verification to make it pass.
