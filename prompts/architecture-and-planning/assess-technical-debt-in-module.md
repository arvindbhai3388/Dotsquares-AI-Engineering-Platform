# Assess and Prioritize Technical Debt in a Module

**Category:** Architecture & Planning
**Use when:** A team wants to justify and plan a debt-paydown effort to stakeholders.

## Prompt

Assess the technical debt in the module described below and produce a ranked, actionable list — not a vague complaint list — that can be used to justify and plan a paydown effort to stakeholders. This is a read-only analysis: do not refactor or change any code as part of this task.

Read through the module's actual source (not just its documentation) and identify concrete debt items across these categories: duplicated logic, missing or inadequate test coverage around business-critical paths, tight coupling that blocks testability or reuse, outdated or misused patterns relative to the rest of the codebase, missing error handling or swallowed exceptions, performance issues (N+1 queries, unnecessary synchronous blocking, unbounded loops/allocations), security gaps (unparameterized queries, missing authorization checks, logged sensitive data), and outdated or unsupported dependencies.

For each debt item found, produce an entry with:

1. **Description** — what the problem is, with a specific file/method/line reference, not a generalization.
2. **Why it's debt** — the concrete negative consequence today (bugs it has likely caused or could cause, time it costs during changes, risk it poses).
3. **Impact if unaddressed** — what gets worse over time if this is left alone (e.g., grows harder to test, becomes a recurring source of production incidents, blocks a planned future feature).
4. **Effort to fix** — rough size (S/M/L) and what the fix would concretely involve.
5. **Impact of fixing** — what improves (reduced bug rate, faster future changes, unblocks other work) — be specific, not "improves maintainability."
6. **Risk of fixing** — anything that makes the fix itself risky (e.g., no test coverage to fix it safely, on a critical hot path).

After listing all items, produce a **prioritized ranking** using an impact-vs-effort view (e.g., a simple table sorted by impact/effort ratio, calling out "quick wins" vs. "big rocks" vs. "not worth it now"), and a **suggested paydown sequence** that a team could execute incrementally alongside normal feature work rather than as one large separate effort.

Do not begin fixing any of these items until I've reviewed and prioritized the list with you.
