# Produce an Architecture Decision Record (ADR)

**Category:** Architecture & Planning
**Use when:** A decision (e.g., choice of messaging technology, ORM strategy, auth approach) needs to be recorded for future reference.

## Prompt

Produce an Architecture Decision Record (ADR) for the technical decision described below. This is a documentation deliverable only — do not implement the decision as part of this task; the ADR is what gets reviewed and approved first.

Use the standard ADR structure, filled in with real substance specific to this decision (not generic placeholders):

1. **Title** — short, specific, in the form "Use X for Y" or "Adopt X over Y for Z."
2. **Status** — Proposed (since this is pending my approval).
3. **Context** — the forces at play: the problem that triggered this decision, current pain points or limitations in the existing approach, relevant constraints (existing stack, team familiarity, compliance, timeline, budget), and why this decision needs to be made now rather than deferred.
4. **Options considered** — at least two real, named alternatives (including "do nothing" or "keep current approach" if relevant), each with a short description of how it would work in this codebase specifically.
5. **Decision** — the option chosen, stated plainly in one or two sentences.
6. **Rationale** — why this option won over the others, referencing the specific evaluation criteria that mattered (cost, team expertise, performance, maintainability, compatibility with existing architecture, vendor risk) rather than generic praise.
7. **Consequences** — both positive and negative, stated honestly. Explicitly include: what becomes harder or is given up by choosing this option, new operational responsibilities it introduces, and its impact on existing code/patterns.
8. **Alternatives explicitly rejected and why** — one line per rejected option tying back to the evaluation criteria.
9. **Rollback/reversal cost** — how difficult it would be to reverse this decision later if it proves wrong, and what that would involve.
10. **Follow-up actions** — concrete next steps once this ADR is approved (e.g., spike, proof of concept, migration plan), distinct from the ADR itself.

Write the ADR as a standalone Markdown document suitable for checking into an `adr/` or `docs/decisions/` folder. Do not begin implementation until I confirm the decision is approved.
