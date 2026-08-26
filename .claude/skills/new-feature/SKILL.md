---
name: new-feature
description: >
  Use when adding a new feature to any project in this platform or a
  client project built on it — enforces the full analyze → propose →
  approve → implement → test → review workflow end to end, regardless of
  stack (ASP.NET Core, MVC, Razor Pages, Blazor, Umbraco, EF Core, SQL
  Server, SignalR, Power BI, SharePoint, Power Apps). Trigger phrases:
  "add a new feature", "implement this requirement", "build this from
  scratch", "add support for X". This is the platform's flagship workflow —
  every feature addition should go through it rather than jumping straight
  to code.
---

# New Feature Workflow

This is the canonical implementation of the platform's core discipline
(see root `README.md` and `wiki/AI-Workflow-Discipline.md`):

```
Analyze → Propose → Approve → Implement → Test → Review
```

Do not skip steps. Do not implement before the plan is approved. A
feature that "seems simple" still goes through Understand and Plan — the
discipline is what prevents ad hoc prompting, not a judgment call to make
per task.

## Step 1 — Understand

- Restate the requirement in your own words: what capability is being
  added, who uses it, what does success look like.
- Identify which stack(s)/project(s) it touches (a client's ASP.NET Core
  API, a Blazor front end, a SQL Server schema change, a Power BI embed,
  etc.) — a feature often spans more than one.
- Note explicit constraints: backward compatibility requirements, non-
  functional requirements (performance, security posture), and anything
  the requester has already ruled in/out.
- Do not assume implementation details not stated — if the requirement
  is ambiguous on something that materially changes the design (e.g.,
  "should this be real-time" implying SignalR vs a polling endpoint), ask
  or state the assumption explicitly before proceeding.

## Step 2 — Locate

- Find the relevant existing code: similar features already implemented
  in the target project, the layer(s) that will need to change
  (controller/page/component, service, data access, external
  integration).
- Search within the smallest relevant scope first (the specific project/
  module), expanding only if the feature genuinely spans more.
- Identify existing services/utilities/patterns to reuse — do not plan a
  new abstraction where an existing one already does the job.
- If a matching stack-specific agent exists for the layer being touched
  (aspnet-core-developer, efcore-developer, blazor-developer, etc.),
  consult it for the idioms/pitfalls relevant to that layer while
  planning, not just when writing code.

## Step 3 — Plan

Produce a concrete plan before writing any code — `templates/pre-implementation-checklist.md`
is the client-repo-facing copy of this same gate; keep the two in sync if either changes.

- The layers/files that will change (new + modified), stated explicitly.
- The data/schema impact, if any (new columns/tables → note whether
  expand/contract is needed, see the efcore-migration skill).
- The public contract impact, if any (new/changed API endpoints,
  SignalR client methods, connector operations) — additive vs breaking,
  stated explicitly.
- The test strategy: which test project, which framework (detect, don't
  assume — see the unit-testing skill), which scenarios need coverage
  (success, validation failure, authorization failure, cancellation). If
  this project has configured `<QA_ARTIFACTS_FOLDER>`, hand this strategy
  to the `qa-test-tracking` skill to save the planned test cases to the
  manual-QA workbook before Implement begins.
- Any new dependency being introduced, with justification (per the
  platform's dependency-decision principle: prefer existing capabilities
  first).
- Security-sensitive surface introduced (new external input, new auth
  boundary, new external integration) — flag explicitly so a
  security-reviewer pass is warranted at Review.

## Step 4 — Approve

- Present the plan to the requester before implementing anything
  non-trivial. This is the platform's core guarantee: **AI proposes, a
  developer approves, then implementation happens** — do not skip
  straight from Plan to Implement on a real feature (a one-line, truly
  trivial change is the only exception, and even then state what you're
  about to do before doing it).
- If operating in an automated/batch context where interactive approval
  isn't available, make the plan explicit in output before implementing,
  so the approval step is at minimum visible and reviewable after the
  fact, and proceed only as far as the task's own instructions authorize.
- The developer may approve with a single Yes/No that carries through
  Implement → Test → Review without further per-step check-ins — see
  `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" for exactly what
  that does and does not authorize (it never covers commit/push).

## Step 5 — Implement

- Follow the approved plan; if reality diverges from the plan once
  you're in the code (a false assumption surfaces), pause and reconcile
  rather than silently improvising a different design.
- Use the matching stack-specific agent's idioms (DI lifetimes, async
  patterns, framework-specific pitfalls) for each layer touched.
- Change only what the plan called for — no unrelated refactors,
  renames, or formatting changes bundled in.
- Never hardcode secrets/connection strings; use the project's
  configuration/options pattern (see platform CLAUDE.md §2 for this
  repo's own rule, or the client project's equivalent).

## Step 6 — Test

- Write tests **before or alongside** implementation for the core
  behavior (Test-First where practical) — see the unit-testing skill for
  the detect-framework-first discipline.
- Cover success, validation/bad-input, failure, and authorization paths;
  add cancellation-token coverage for async APIs that accept one.
- Confirm new tests fail for the right reason before the implementation
  exists (if genuinely done Test-First), then pass once it's complete.
- If a `qa-test-tracking` workbook was created at Plan time, update it now
  with real Actual Result/Pass-Fail values from the test run just
  performed — never from assumption.

## Step 7 — Review

- Run (or hand off to) `code-reviewer` for the general checklist
  (correctness, security, nullability, error handling, performance,
  maintainability, backward compatibility, unintended changes).
- Run (or hand off to) `security-reviewer` if the feature touches auth,
  external input, deserialization, outbound calls, or secrets.
- Run (or hand off to) `performance-reviewer` if the feature touches a
  hot path, a new query pattern, or an external call in a loop — not
  every change needs this, only ones with real performance surface.
- Run (or hand off to) `production-safety` for a production/database/
  auth/major-API/critical-integration/large-refactor/high-risk change —
  it has authority to BLOCK; do not consider the feature done over its
  objection.
- Run (or hand off to) `build-validator` to actually build/test the
  affected project(s) with the correct toolchain before calling the
  feature done.
- For the final answer, `quality-gate` can aggregate all of the above
  into one PASS/WARN/FAIL instead of relaying each reviewer's output
  separately.
- Report back clearly what was verified and what wasn't (e.g., "unit
  tests pass; did not verify against a live SharePoint tenant since
  demos use a mock client").

## Don't
- Don't implement before the plan is stated/approved.
- Don't skip Test in favor of "it looks right."
- Don't claim Review passed without actually running the review/build
  steps.
- Don't bundle unrelated changes into a feature implementation.
