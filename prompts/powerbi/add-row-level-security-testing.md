# Add RLS Verification: View As Roles Plus Automated Check

**Category:** Power BI
**Use when:** RLS rules need to be verified before a report ships to production.

## Prompt

Add a verification approach for Row-Level Security (RLS) roles on a Power BI dataset before it ships to production, combining the manual Power BI Desktop "View As Roles" check with an automated REST API-based check that can run as part of this app's existing test/CI process. RLS bugs are a data-security issue, not just a cosmetic one -- a misconfigured role can leak one customer's data to another -- so treat verification here as seriously as the RLS implementation itself.

Two parts:

1. **Manual verification checklist (document, don't automate):** Describe the steps to use Power BI Desktop's "View As Roles" feature against the published dataset: select each defined role, optionally combined with a specific username/UPN via "Other user", and confirm the visuals show only the expected filtered rows for representative test identities (at least one identity per role, plus one identity that should match zero roles to confirm it sees no data rather than falling back to unfiltered access). Call out that this must be re-run whenever the DAX filter expression or table relationships change, not just at initial setup.

2. **Automated check (implement in code):** Using the Power BI REST API's "Execute Queries In Group" endpoint or the dataset's `GenerateToken` + a subsequent DAX query with `EffectiveIdentity` set, write an automated test that programmatically requests data as a specific test identity/role and asserts the returned row set matches the expected filtered subset (e.g. only rows for that identity's organization ID). Reuse this app's existing service-principal auth code rather than duplicating it. Place this test in whichever test project already covers Power BI integration, following its existing naming/Arrange-Act-Assert conventions; if no such project exists yet, propose where it should live and confirm with me before creating one.

Explicitly test the negative case: an identity with no matching role assignment should see zero rows, not all rows -- this is the most common and most dangerous RLS misconfiguration (a role that fails to filter is worse than no role at all, since it creates false confidence). Also verify role names in code match the dataset's role names exactly (case-sensitive), since a typo silently results in unfiltered access rather than an error.
