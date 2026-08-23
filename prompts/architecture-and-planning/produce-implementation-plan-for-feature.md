# Produce an Implementation Plan for a New Feature

**Category:** Architecture & Planning
**Use when:** Starting a non-trivial ticket that shouldn't be implemented ad hoc.

## Prompt

Before writing any code, produce a structured implementation plan for the feature described below. Treat this as a proposal document for me to review and approve — do not create, edit, or modify any files as part of this task.

First, locate and read the relevant existing code: identify the project(s), modules, controllers/services, and data-access paths this feature will touch, and note the existing patterns (naming, layering, DI usage, error handling) you intend to follow so the new code fits in cleanly rather than introducing a parallel style.

Then produce a plan document with these sections:

1. **Summary** — one paragraph restating the feature in your own words and confirming your understanding of scope and non-scope (explicitly list what is out of scope).
2. **Approach** — the chosen implementation strategy, stated plainly, plus 1-2 alternative approaches you considered and why you rejected them.
3. **Affected files/modules** — a concrete list of files/classes to add or change, grouped by layer (data access, business logic, API/controller, UI, configuration/DI registration).
4. **Data/schema impact** — any new tables, columns, or contracts, and whether they are additive or require migration.
5. **Risks and edge cases** — concurrency, null/validation gaps, backward compatibility, performance under load, security/authorization implications.
6. **Test strategy** — which test project/framework applies, the specific scenarios (happy path, validation failure, authorization failure, cancellation) you will cover, and what cannot reasonably be automated.
7. **Rollout plan** — deployment order, feature flags if applicable, and a rollback plan if something goes wrong post-deploy.
8. **Effort estimate** — rough size (S/M/L or hours) with the main sources of uncertainty called out.

Stop after producing this plan and wait for my explicit approval before touching any code.
