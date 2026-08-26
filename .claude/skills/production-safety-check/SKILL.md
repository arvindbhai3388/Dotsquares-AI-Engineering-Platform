---
name: production-safety-check
description: >
  Use before calling a change production-ready — database/schema,
  restricted-config, breaking API, auth/authz, logging, external-
  integration, backward-compatibility, and rollback checks, with
  authority to BLOCK. Triggers on "is this production ready", "check
  this before deploy". Use for production/database/auth/major-API/
  critical-integration/large-refactor/high-risk changes.
---

# Production Safety Check Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

Delegate to the `production-safety` subagent with the diff or changed file list.

## Steps
1. Gather the diff (or ask the user for the target files if not obvious).
2. Pass to `production-safety`.
3. Relay the verdict (PASS/WARN/BLOCK) plainly — a BLOCK means the change is **not** done yet,
   regardless of what any other review said. State exactly what would resolve it.
4. For a WARN, confirm the user has actually seen and acknowledged it before treating the change
   as complete — a WARN silently ignored is functionally the same mistake a BLOCK prevents.

## Do not
- Do not downgrade a BLOCK to a WARN on your own judgment — that authority belongs to the
  `production-safety` agent's actual review, not to this skill's discretion.
- Do not skip this for a change matching its trigger criteria just because time is short.
