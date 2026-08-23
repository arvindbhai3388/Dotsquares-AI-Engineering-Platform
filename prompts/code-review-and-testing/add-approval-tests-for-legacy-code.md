# Add Approval Tests for Legacy Code

**Category:** Code Review & Testing
**Use when:** Refactoring legacy code with no tests and unclear/undocumented intended behavior.

## Prompt

Add characterization (approval) tests for the legacy method/class I specify, before any refactoring begins. The goal here is different from ordinary unit testing: I need to pin down what the code *currently* does, not assert on what it *should* do -- because the intended behavior isn't clearly documented and I can't safely refactor without a safety net that would catch any accidental behavior change.

Approach:

1. Read the code to understand its inputs, outputs, and any side effects (database writes, file output, external calls) -- if it has side effects that are impractical to observe directly in a test, identify the smallest seam needed to capture them (e.g., wrapping a write call so a test can intercept what would have been written) without changing behavior.
2. For each meaningfully distinct input scenario you can identify from the code paths (branches, loops, special-cased values), run the actual current implementation and capture its real output as the expected value in the test -- do not hand-derive what the "correct" output should be, since that's exactly the ambiguity this technique sidesteps. If a snapshot/approval testing library is already available in the project, use it to store larger outputs as approved snapshot files; otherwise assert directly on the captured value in the test body.
3. Name these tests clearly as characterization tests (e.g., a `Characterization` suffix on the test class, or a comment at the top of the file) so future readers understand these tests intentionally describe current behavior, not a specification -- this distinction matters so nobody later "fixes" a characterization test to match what they assume the code should do without realizing that changes its purpose.
4. Cover enough of the method's branches that a refactor changing any of them would be caught -- prioritize breadth of code-path coverage over depth of edge-case coverage, since the point is safety-net coverage before refactoring, not exhaustive correctness verification.

Explicitly flag any captured behavior that looks like it's probably a bug (inconsistent handling of a null vs. empty case, an off-by-one that seems unintentional) -- note it in a comment next to that specific test rather than silently treating it as the desired behavior, so the decision to preserve or fix it is made deliberately, not by default.

Once these tests are in place and passing against current behavior, they become the regression gate for the refactor: run them before and after each refactor step, and treat any change in their output as a signal to stop and investigate, not to update the test.
