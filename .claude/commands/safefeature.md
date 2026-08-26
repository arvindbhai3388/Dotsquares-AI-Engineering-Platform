---
description: Safe Mode - production/database/auth/major-API/critical-integration/high-risk change. Full pipeline, mandatory production-safety and quality-gate, no shortcuts.
argument-hint: <description of the change>
---

Safe Mode for a production/database/auth/major-API/critical-integration/large-refactor/
high-risk change: $ARGUMENTS

This explicitly **disables** Streamlined mode's single-Yes/No shortcut for this task — every
step (Analyze, Propose, Approve, Implement, Test, Review) gets its own checkpoint, even if the
developer would otherwise fast-track it, because the entire point of invoking Safe Mode is that
this specific change deserves the full ceremony, not the shortened version.

## Required, non-negotiable for this mode

1. Run the full `new-feature` workflow (Understand → Locate → Plan → Approve → Implement → Test
   → Review) with no steps compressed or skipped.
2. Run `production-safety` (`/production-safety-check`) before considering this done, regardless
   of how safe the change looks going in — its authority to BLOCK exists specifically for changes
   like this one.
3. If `production-safety` returns `BLOCK`, treat the task as **not done** — report the block and
   exactly what would resolve it; do not proceed past that point on your own judgment.
4. Run `quality-gate` at the end to produce one aggregated PASS/WARN/FAIL verdict rather than
   relaying each reviewer's raw output separately.
5. If the change touches a database schema, use the `efcore-migration` skill's expand/contract
   discipline explicitly — a single-step destructive migration is not acceptable in Safe Mode.
6. If the change touches auth/authorization, treat any authorization check being removed or
   weakened as an automatic stop-and-confirm point, per `production-safety`'s own checklist.

## Report

State the final `quality-gate` verdict clearly and explicitly at the end — "PASS", "WARN" with
what's still open, or "FAIL" with what's blocking — not a vague "looks good."
