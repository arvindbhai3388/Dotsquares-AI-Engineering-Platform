# Evaluate Build-vs-Buy for an Integration Need

**Category:** Architecture & Planning
**Use when:** A new capability is needed and it's unclear whether to build it in-house or adopt an existing product/service.

## Prompt

Evaluate build-vs-buy for the integration capability described below. Produce a decision-support document for me to review — do not install any package, provision any service, or write integration code as part of this task.

Structure the evaluation as follows:

1. **Requirement summary** — restate the actual capability needed (not a specific product), including functional requirements, expected volume/scale, and any hard constraints (compliance, data residency, on-prem requirement, budget ceiling).
2. **Current stack fit** — check whether an existing dependency already in this solution (or a very close relative already used elsewhere in similar .NET solutions) could satisfy this need with reasonable effort, so we don't buy or build something that duplicates an existing capability.
3. **Build option** — what building it in-house would involve: rough component list, the ongoing maintenance burden (who owns it, on-call/patching, scaling work), and the main technical risks of getting it wrong (e.g., message durability for a hand-rolled queue, relevance tuning for a hand-rolled search index).
4. **Buy/adopt option** — name 2-3 realistic candidate products/services (self-hosted or managed) that fit the requirement, and for each: licensing/cost model, operational footprint (does it need its own infra to run/patch), integration effort into this codebase, vendor lock-in risk, and maturity/community support.
5. **Comparison table** — build vs. each buy candidate across: upfront cost, ongoing cost, time-to-first-working-version, maintenance burden, scalability headroom, and security/compliance fit.
6. **Recommendation** — a clear recommendation with the top 2-3 reasons, explicitly stating the assumptions the recommendation depends on (e.g., "assumes volume stays under X/day").
7. **Risks of the recommended path and mitigation** — including an exit strategy if the chosen option turns out to be wrong later (how hard would it be to switch).
8. **Rollback/reversibility** — how reversible this decision is once implemented, and what a later migration away from it would cost.

Do not proceed to implementation or dependency installation until I've approved a direction.
