---
name: quality-gate
description: >
  Use as the final step before calling any change done — aggregates
  build-validator/code-reviewer/security-reviewer/performance-reviewer/
  production-safety results (already run this task) into one PASS/WARN/
  FAIL verdict. Triggers on "is this ready to ship", "final check before
  I close this out".
---

# Quality Gate Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

Delegate to the `quality-gate` subagent, giving it what's already been reported this task by
`build-validator`, `code-reviewer`, `security-reviewer`, `performance-reviewer`, and
`production-safety` (whichever of those actually ran — not every change needs all of them).

## Steps
1. Confirm which reviewers actually ran this task and what they found — don't guess or assume a
   category passed silently.
2. Pass that to `quality-gate` for synthesis into one verdict.
3. Relay the verdict plainly. A `FAIL` means the change is not done — state what's blocking it.
4. If `quality-gate` had to spot-check a build/test itself because that status was missing or
   stale, note that explicitly so the user knows it wasn't just relayed from an earlier claim.

## Do not
- Do not treat this as a substitute for actually running `/code-review`, `/security-review`, etc.
  first — it aggregates existing results, it doesn't generate them.
- Do not render a PASS verdict yourself if `quality-gate` reported FAIL or an unresolved BLOCK.
