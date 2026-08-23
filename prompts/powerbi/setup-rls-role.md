# Define and Apply a Row-Level Security Role

**Category:** Power BI
**Use when:** Different users must only see their own organization's/region's data in an embedded report.

## Prompt

Design and apply Row-Level Security (RLS) for an embedded Power BI report so that each authenticated user only sees rows belonging to their own organization or region. Start with Understand -> Locate -> Plan: identify how the app currently maps an authenticated user to their organization/region (claims, database lookup, tenant ID), and confirm the report's dataset already has (or needs) an RLS role defined in Power BI Desktop before touching any code. Propose the plan and wait for my approval before implementing.

Cover both halves of the problem:

1. **Dataset-side (Power BI Desktop / dataset):** Describe the DAX filter expression needed on the relevant table (e.g. `[OrganizationId] = USERPRINCIPALNAME()` or a lookup against a security table keyed by `USERNAME()`/`USERPRINCIPALNAME()`), and name the role clearly (e.g. "OrgAccess"). Note that this part is authored in Power BI Desktop/the Power BI service, not in application code -- call this out explicitly rather than trying to generate DAX files as if they were application source.

2. **Application-side (.NET):** When generating the embed token for a given user, pass an `EffectiveIdentity` object containing the username (or a stable identifier such as the org ID) and the exact role name(s) defined in the dataset, plus the target dataset ID(s). Ensure this identity is derived from the authenticated user's server-side session/claims -- never from a client-supplied parameter that the browser could tamper with, since that would let a user impersonate another organization's identity and defeat RLS entirely.

Explicitly flag these edge cases in your implementation and in code comments:
- What happens if the role name in code doesn't match the dataset's role name exactly (silent failure to filter, not an error) -- add a test or startup validation if feasible.
- Multiple roles for a single user (roles are combined with OR logic in Power BI) -- confirm this is the desired behavior for this use case.
- The dataset must have RLS enabled and at least one role for `EffectiveIdentity` to have any effect; embedding without a matching role silently shows unfiltered data to users with dataset-level access.

Write or update tests validating that the embed-token service correctly attaches the expected role and identity value for a given test user before implementing, per Test-First.
