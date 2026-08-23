# Generate xUnit Tests for an Existing Method

**Category:** Code Review & Testing
**Use when:** A method has no test coverage and needs a safety net before refactoring.

## Prompt

Generate a thorough xUnit test suite for the method I point you to (name the class, method signature, and file). Before writing any test, read the method body plus its direct callers and dependencies so you understand its actual contract -- do not guess behavior from the method name alone.

Structure every test using Arrange-Act-Assert with clear section comments or blank-line separation, and name tests using the `MethodName_Scenario_ExpectedResult` convention (or whatever convention the existing test project already uses -- check for one first and match it rather than introducing a new style).

Cover these categories explicitly, and tell me which ones do not apply and why:

- **Happy path** -- typical valid input produces the expected output.
- **Boundary values** -- empty strings/collections, zero, negative numbers, min/max values, single-element collections, off-by-one conditions around loop or range logic.
- **Null and default handling** -- null arguments, null properties on input objects, default(T) for value types where relevant.
- **Failure paths** -- invalid input that should throw or return an error result; verify the exact exception type and message/error code, not just "it throws something."
- **Dependency interactions** -- if the method calls injected collaborators, use test doubles (Moq fakes/stubs) to isolate the method under test from real implementations, and verify both the returned value and, where behavior depends on it, that collaborators were invoked with the expected arguments and call count.
- **Async-specific concerns** -- if the method is async, test cancellation via `CancellationToken`, and confirm the method actually awaits rather than blocking.

Keep each test independent: no shared mutable static state between tests, no reliance on execution order, and no test that depends on another test having run first. Prefer one logical assertion focus per test over one giant test that checks everything.

After writing the tests, run them and report the actual pass/fail results -- do not claim they pass without executing them. If a test fails because it exposes a real bug in the method (not a bad test), stop and flag it to me before "fixing" the test to match broken behavior.
