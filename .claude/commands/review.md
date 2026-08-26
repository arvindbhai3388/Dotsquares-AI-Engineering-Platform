---
description: Quick review - code-review always, plus security/performance review where the diff actually touches that surface, synthesized by quality-gate.
argument-hint: "<optional - specific files/PR to review; otherwise the current uncommitted changes>"
---

Review the diff or changed files: $ARGUMENTS (if not specified, review the current uncommitted
changes or whatever change was most recently discussed in this session).

## Steps

1. Always run `code-review` (`code-reviewer`) — the standing correctness/maintainability
   checklist applies to every change, regardless of what else is relevant.
2. Only if the diff actually touches the relevant surface, also run:
   - `security-review` (`security-reviewer`) — auth, external input, deserialization, outbound
     calls, secrets.
   - `performance-review` (`performance-reviewer`) — a new query pattern, a hot path, an
     external call in a loop. Don't run this on a diff with no performance-relevant surface.
   - This project's data-access review skill/agent, if it has one, for schema/query changes.
3. Synthesize everything through `quality-gate` into one PASS/WARN/FAIL rather than relaying
   each reviewer's raw output separately as a wall of disconnected findings.
4. Do not apply any suggested fix automatically — report findings and wait for explicit
   approval, the same as every other review skill on this platform.
