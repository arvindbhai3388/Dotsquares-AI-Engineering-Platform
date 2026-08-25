---
name: qa-test-tracking
description: >
  Use to maintain a manual-QA Excel test-case workbook alongside the
  automated Test-First/Validate cycle for any project on this platform —
  auto-saved before implementation starts (planned test cases) and
  auto-updated with real Pass/Fail results after Validate runs. Trigger
  phrases: "track this in the QA workbook", "update the test case sheet",
  "generate the test case matrix" — and runs by default as part of the
  new-feature workflow's Plan and Test steps once a project has configured
  a QA artifacts folder. Distinct from the unit-testing skill's automated
  xUnit/MSTest/NUnit tests — this produces a human-readable manual-QA
  artifact, not code. Delegates actual spreadsheet creation/editing to the
  `xlsx` skill.
---

# QA Test-Case Tracking Workflow

This is a **manual-QA artifact**, separate from the automated tests the `unit-testing` skill
writes — it exists so a non-technical stakeholder (QA lead, client, project manager) can see the
test plan and its real results without reading code or a test runner's console output. Only runs
for a project that has configured `<QA_ARTIFACTS_FOLDER>` in its `CLAUDE.md` (see
`templates/CLAUDE-full.md`/`CLAUDE-minimal.md`) — if that placeholder is still unfilled, skip this
skill entirely and say so, rather than guessing a save location.

## Step 1 — Before implementation (at Plan time)

Once the `new-feature` skill's Plan step has produced a test strategy (which scenarios need
coverage: success, validation failure, authorization failure, cancellation, etc.):

1. Check whether a workbook for this ticket/feature already exists at
   `<QA_ARTIFACTS_FOLDER>\TC_<Ticket>_<ShortCode>_UnitTestCases.xlsx` (naming convention: the
   project's own ticket/short-code scheme if it has one, otherwise a short slug derived from the
   feature name). If it exists, update it rather than overwriting from scratch.
2. Using the `xlsx` skill, create/update a **two-sheet workbook**:
   - **Test Case Matrix** sheet — one row per planned test case, columns: `Test Case ID`,
     `Description`, `Steps`, `Expected Result`, `Actual Result` (blank until Step 2 below),
     `Pass/Fail` (blank until Step 2 below), `Notes`.
   - **Execution Dashboard** sheet — a summary: total test cases, passed, failed, not-yet-run,
     with a simple chart/conditional-formatting pass-rate indicator, plus rebuilt automatically
     whenever the matrix sheet changes.
3. Apply a clean, readable format — a styled header row (bold, background fill), frozen header row,
   auto-sized columns, conditional formatting on the `Pass/Fail` column (green/red) once results
   exist. This is a stakeholder-facing document; treat its readability as part of "done," not an
   afterthought.
4. Save it to `<QA_ARTIFACTS_FOLDER>` — **never inside the project's own repo** (this is an
   external, non-source artifact, same principle as build output never being committed).

## Step 2 — After Validate (real results only)

Once the `unit-testing` skill's Validate step has actually run the test suite and you have real
pass/fail output:

1. Open the same workbook from Step 1.
2. For each row in the Test Case Matrix that corresponds to a test that was actually run, fill in
   `Actual Result` (a one-line description of what actually happened) and `Pass/Fail` — **only
   based on a test run you actually executed**, never inferred or assumed. If a planned test case
   was never actually implemented/run, leave it blank and flag that gap explicitly rather than
   marking it Pass by default.
3. Recompute the Execution Dashboard sheet's totals.
4. Report back which ticket's workbook was updated and its final pass rate.

## Do
- Only mark Pass/Fail from a test you actually ran — this mirrors the platform's core rule of never
  claiming a build/test passed without running it.
- Keep the workbook outside the repo, in the project-configured `<QA_ARTIFACTS_FOLDER>`.
- Update an existing ticket's workbook rather than creating a duplicate for the same ticket.

## Don't
- Don't guess a save location if `<QA_ARTIFACTS_FOLDER>` isn't configured — skip and say so.
- Don't mark a test case Pass without having actually executed it.
- Don't treat this as a substitute for the automated tests the `unit-testing` skill writes — both
  exist, for different audiences.
