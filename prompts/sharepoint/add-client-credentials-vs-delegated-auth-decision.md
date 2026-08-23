# Decide Between Client Credentials and Delegated Auth for a Graph Feature

**Category:** SharePoint (Microsoft Graph)
**Use when:** Setting up auth for a new Graph-based feature and it's unclear whether it should run as the app or as the signed-in user.

## Prompt

I'm adding a new SharePoint/Graph-backed feature and need to decide the authentication model before writing any client setup code. Analyze the scenario I describe (who initiates the operation, whether it needs to run unattended/on a schedule, and whether results must be scoped to what a specific signed-in user is allowed to see) and recommend client credentials (app-only) versus delegated (on-behalf-of the signed-in user) authentication for Microsoft Graph.

Cover in your analysis:
1. Client credentials flow: appropriate for background workers, scheduled jobs, or service-to-service calls with no interactive user present. Note that permissions granted this way apply tenant-wide (or to whatever sites `Sites.Selected` was scoped to) regardless of which end user triggered the workflow — flag this clearly if the feature has any per-user data-visibility requirement, since app-only auth will not automatically filter results to what that user could see.
2. Delegated flow: appropriate when an interactive user is present and results/actions must respect that user's own SharePoint permissions. Identify which delegated flow fits this app's architecture — authorization code flow for a web app with a browser-based sign-in, or on-behalf-of flow if this app is itself an API being called by another authenticated client — and check what this app already uses for user sign-in (e.g., existing Microsoft.Identity.Client/MSAL or ASP.NET Core auth setup) rather than introducing a second auth stack.
3. Token lifetime and caching implications: delegated tokens expire and require refresh tokens or MSAL's token cache; client credentials tokens can be cached and reused across requests via `ConfidentialClientApplication` until near expiry. Recommend the caching approach appropriate to the chosen flow.
4. Security tradeoffs: app-only credentials (secret or certificate) are a standing, powerful credential that must never be hardcoded or logged; delegated flows push authorization decisions to Azure AD/SharePoint per user, which is generally safer when feasible.

Give a clear recommendation with reasoning, not both options hedged. Follow the analyze -> propose -> approve -> implement -> test -> review workflow: present the recommendation and wait for my approval before implementing the `GraphServiceClient`/`ConfidentialClientApplication` construction, since this decision affects the app registration's permission type (application vs delegated permissions) and is hard to change later without re-registering.
