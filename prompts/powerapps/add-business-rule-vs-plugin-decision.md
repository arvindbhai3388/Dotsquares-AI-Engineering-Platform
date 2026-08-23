# Decide Where New Dataverse Logic Belongs: Business Rule, Workflow, or Plugin

**Category:** Power Apps / Power Platform
**Use when:** It's unclear which customization mechanism is appropriate for new business logic.

## Prompt

Analyze the piece of logic I describe (state the trigger condition and the desired effect precisely, e.g. "when Status changes to Approved, set ApprovalDate and lock three fields" or "when a Contact is created, default its Owner from the parent Account") and decide whether it belongs in a Dataverse business rule, a classic (background) workflow, or a plugin. Do not implement until the decision is confirmed with me.

Use this decision framework and state which criteria pushed you toward the final answer:
- **Business rule** fits when: the logic is simple field-level UI behavior (show/hide, set value, set business-required, lock field, field-level validation message) that should also apply live in the form before save, doesn't need to call external code or perform complex branching beyond what the rule designer supports, and doesn't need to run on Delete or on operations outside the form/save pipeline.
- **Classic workflow** fits when: the logic is a simple sequence of record updates/checks/wait conditions that doesn't need sub-second responsiveness, benefits from being editable by a power user without redeployment, or needs to run on a recurring schedule -- but note that classic workflows are legacy relative to Power Automate cloud flows, so if this is new logic (not a fix to something existing), recommend a cloud flow instead and say why.
- **Plugin** fits when: the logic must run synchronously and block the save (validation that must prevent an invalid state, calculated fields that must be correct before the transaction commits), needs complex conditional/loop logic, needs to call external services or perform operations the low-code designers can't express, needs to run on messages business rules/workflows can't hook (e.g. Merge, Assign, Associate/Disassociate), or has performance/testability requirements that argue for compiled, unit-testable code.
- Flag if the requirement actually spans more than one mechanism (e.g. a business rule for the live form experience plus a plugin for server-side enforcement of the same constraint), since relying on client-side-only validation is a common security gap.

Present the recommendation with the reasoning above as a short table, wait for my approval, then implement using the matching skeleton/pattern for whichever mechanism we land on (plugin skeleton with Test-First unit tests if a plugin; the appropriate configuration steps if a business rule or workflow, called out as manual designer steps rather than a code diff).
