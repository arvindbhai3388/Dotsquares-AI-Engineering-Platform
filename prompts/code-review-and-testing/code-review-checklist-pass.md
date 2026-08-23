# Run a Full Code Review Checklist Pass

**Category:** Code Review & Testing
**Use when:** Doing a final review pass before a PR is marked ready.

## Prompt

Perform a complete code review pass over the diff (staged changes, a specific commit range, or the PR I point you to). Go through each dimension below in order and report findings under that heading -- don't skip a section just because the first issue you found was elsewhere.

1. **Correctness** -- does the logic do what the PR description/ticket claims? Check boundary conditions, off-by-one errors, incorrect operator usage (`&&` vs `||`, `<` vs `<=`), and whether edge cases (empty input, null, zero, max values) are handled the way the rest of the codebase handles them.
2. **Nullability** -- for nullable reference type-enabled projects, check for suppressed warnings (`!`) that hide a real null risk, missing null checks on inputs from external callers (API request bodies, deserialized data), and inconsistent null-handling between similar methods.
3. **Error handling** -- are exceptions caught at the right layer, not swallowed silently, not used for ordinary control flow, and not caught so broadly (`catch (Exception)`) that real bugs get hidden? Are errors surfaced to callers with enough information to act on, without leaking internal details (stack traces, connection strings) to untrusted callers?
4. **Performance** -- N+1 query patterns from loading a collection then querying per-item, unnecessary allocations in hot paths, synchronous blocking calls (`.Result`, `.Wait()`) on async code, missing pagination on endpoints returning potentially large result sets.
5. **Maintainability** -- duplicated logic that should reuse an existing helper/service, unclear naming, methods doing too many unrelated things, magic numbers/strings that should be named constants, and whether the change follows the existing architectural patterns in the surrounding project rather than introducing a new style.
6. **Backward compatibility** -- does the change alter a public method signature, API response shape, status code, or database schema in a way that could break existing callers or overlapping deployed versions?
7. **Unintended changes** -- diff noise from reformatting, unrelated file touches, or accidental reverts of someone else's recent change.

For every finding, cite file and line, state severity (Blocker/Major/Minor/Nit), and give a concrete fix suggestion. End with a clear overall verdict: approve, approve with minor comments, or changes requested -- and why. Do not silently fix issues while reviewing; report them and propose fixes, then wait for approval before implementing.
