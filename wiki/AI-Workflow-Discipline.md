# AI Workflow Discipline

Every agent, skill, and prompt in this platform is built around one non-negotiable sequence:

```
Analyze → Propose → Approve → Implement → Test → Review
```

This page explains why each step exists, what realistically goes wrong when a step is skipped, and how `.claude/skills` enforce the sequence in practice rather than leaving it as a suggestion developers can bypass under deadline pressure.

## Why a fixed sequence at all

An LLM-based coding agent is fast, confident, and occasionally wrong in ways that look identical to being right — a plausible-looking diff that compiles is not the same thing as a correct diff. Left unconstrained, the fastest path for an agent is to jump straight from a one-line request to a multi-file implementation. That is precisely the path that produces the most expensive mistakes, because nobody with full context on the request ever looked at the plan before code started changing. The six-step sequence exists to insert human judgment at the two points where it is cheapest to apply — before implementation (Approve) and after it (Review) — rather than the one point where it is most expensive: after the change has already shipped.

## The six steps

### 1. Analyze

The agent reads the relevant code, identifies the actual project(s)/layer(s) affected, and states its understanding of the current behavior before proposing anything. It does not assume; it locates the real implementation.

**What goes wrong if skipped:** the agent proposes a fix based on a plausible guess about how the code works rather than how it actually works — e.g., "fix" a bug in a controller when the real defect is in a service three layers down, leaving the actual bug in place while adding unrelated code on top of it.

### 2. Propose

The agent states the planned change in plain terms — root cause (for a bug) or approach (for a feature), which files it expects to touch, and any trade-offs — before writing code. For non-trivial work this should identify the smallest correct change, not the most thorough rewrite the agent can imagine.

**What goes wrong if skipped:** scope creep. An agent asked to fix a null-reference exception "helpfully" refactors the surrounding class, renames variables, and upgrades a package on the way past, turning a one-line fix into a 40-file diff nobody asked for and nobody can review in the time available.

### 3. Approve

A human developer reads the proposal and explicitly says go — or redirects it — before any file is written. This is the single most important gate in the sequence because it is the last point where correcting course costs nothing.

**What goes wrong if skipped:** the agent implements the wrong interpretation of an ambiguous ticket. For example, "add caching to the search endpoint" could mean in-memory `IMemoryCache`, a distributed Redis cache, or output caching at the HTTP layer — three architecturally different answers. Without an approval gate, the agent picks one, implements it fully, and the mismatch is only caught in review, after the wrong plumbing (dependency, configuration, tests) is already in place and has to be unwound.

### 4. Implement

The agent writes the smallest correct change matching the approved proposal, following existing project conventions rather than introducing new ones, and touching only the files the proposal said it would touch.

**What goes wrong if skipped (i.e., implementation happens without a matching approved proposal):** this collapses back into the Propose-skipped failure mode — undisciplined scope, or a change that doesn't match what was actually approved.

### 5. Test

Tests are written or updated to pin down the expected behavior — ideally *before* the implementation is written (test-first), so the test can be confirmed to fail for the right reason first. After implementation, the full relevant suite is run for real, not asserted to pass from memory.

**What goes wrong if skipped:** silent regressions. An agent can report "this should work" with total confidence about a change that breaks an edge case (null input, concurrent access, an authorization boundary) it never exercised — because generating plausible code and verifying correct behavior are different activities, and only one of them was done.

### 6. Review

Before any change is considered done, it is checked against a standing checklist: correctness, security, nullability, error handling, performance, maintainability, backward compatibility, and unintended changes. This is the last automated/semi-automated gate before a human does final sign-off and decides whether to commit.

**What goes wrong if skipped:** defects that pass tests but shouldn't ship anyway — a query with an N+1 pattern that works fine on a 10-row dev database and falls over in production, a secret accidentally logged, an API response shape that breaks an existing client. Tests confirm the code does what the tests check; review catches what nobody thought to write a test for.

## Enforcement via `.claude/skills`

The discipline above is not just documentation — it is encoded as executable workflow steps in `.claude/skills/`, so following it is the path of least resistance rather than an extra step a developer has to remember:

- A **new-feature** or **bugfix** skill walks the Analyze → Propose sequence explicitly, producing a written plan before touching any file, and stops for human approval before Implement begins.
- A **unit-testing** skill is invoked automatically before non-trivial implementation (Test-First) and again after (Validate), so Test isn't left to the developer's discretion.
- A **code-review** skill runs the standing Review checklist against the actual diff, rather than relying on the implementing agent to grade its own work.
- A **build-validation** skill is the final gate — it actually builds and runs the affected project's tests with the correct toolchain, and refuses to claim success without having done so.

Because these are skills rather than informal habits, invoking `/new-feature`, `/unit-testing`, `/code-review`, or `/build-validation` reliably produces the same sequence of gates regardless of which developer or which client project is running it — which is the entire point of standardizing this across 50+ developers.

## Related pages

- [Architecture Overview](Architecture-Overview.md) — the layered model this discipline is applied against.
- [Onboarding Guide](Onboarding-Guide.md) — how a new developer starts using this discipline day one.
- [docs/FAQ.md](../docs/FAQ.md) — "can AI commit code for me" and related process questions.
