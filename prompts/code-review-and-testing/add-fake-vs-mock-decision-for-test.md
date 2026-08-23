# Decide Between a Fake, a Mock, and a Stub

**Category:** Code Review & Testing
**Use when:** It's unclear how to isolate a unit under test from a particular dependency.

## Prompt

Analyze the specific dependency I point you to on the class/method under test, and decide deliberately which kind of test double is the right tool -- a stub, a mock, or a fake -- rather than defaulting to Moq for everything. Explain the reasoning, then implement the chosen double correctly.

Use this decision process:

- **Stub** -- use when the test only needs the dependency to return a canned value so the method under test can proceed, and the test doesn't care how or whether the dependency was called. Simplest option; prefer it whenever the test's assertion is entirely about the method's return value or resulting state, not about its interaction with this dependency.
- **Mock** -- use when the *correctness being tested* depends on the interaction itself: the method must call this dependency, with specific arguments, a specific number of times, or must NOT call it under some condition. Set up the mock (via Moq) with `Setup` for any return value needed to drive execution, and `Verify` for the interaction being tested -- but avoid over-verifying incidental calls that aren't the point of this specific test, since that produces brittle tests that break on unrelated refactors.
- **Fake** -- use when a stub/mock would be awkward because the dependency's behavior across multiple calls needs to be internally consistent (e.g., an in-memory repository that must actually store what's "saved" so a subsequent "get" in the same test returns it, rather than each call being independently stubbed). Implement the fake as a small, real (but simplified) implementation of the interface -- e.g., an in-memory `Dictionary`-backed repository -- kept in the test project's shared test-helpers location if one exists.

Apply this specifically to the dependency I named: state which category it falls into, why, and what would go wrong if you picked one of the other two options for it (e.g., "a stub here would miss that the audit call must happen exactly once with the mutated entity, which is the actual bug this ticket is about" or "a mock here would produce a brittle test that breaks on any internal refactor of how many times we look up the cache, when the test only cares about the final resolved value").

Implement the chosen double, wire it into the test via constructor injection (matching however the class under test already receives this dependency), and confirm the resulting test still follows Arrange-Act-Assert and remains independent of other tests (no shared mutable fake state across test methods unless deliberately reset in setup). Run the test and report the actual result.
