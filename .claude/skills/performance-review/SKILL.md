---
name: performance-review
description: >
  Use for a performance-focused review of changed files — N+1 queries,
  blocking async calls, missing pagination, caching gaps. Triggers on
  "review this for performance", "will this scale". Only invoke for
  performance-relevant or explicitly high-risk changes, not every diff.
---

# Performance Review Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

Delegate to the `performance-reviewer` subagent with the diff or changed file list.

## Steps
1. Gather the diff (or ask the user for the target files if not obvious).
2. Confirm this change actually warrants a performance pass — a new data-access path, a hot-path
   change, a loop over external calls, or an explicit request. Skip for changes with no
   performance surface rather than running this on everything.
3. Pass to `performance-reviewer`.
4. Relay findings ranked by severity. Confirm with the user before applying any suggested fix —
   this skill reports, it doesn't auto-fix.

## Do not
- Do not run this on every routine change — it's targeted, not a default gate like `/code-review`.
- Do not resolve findings automatically without the user's go-ahead.
