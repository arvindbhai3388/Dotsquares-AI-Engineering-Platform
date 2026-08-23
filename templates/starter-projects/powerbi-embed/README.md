# Starter Scaffold — Power BI Embedded Analytics

> Template outline for bootstrapping Power BI report embedding inside an ASP.NET Core
> application ("App Owns Data" embedding for customers, most common in client SaaS
> products). This is a folder-structure and setup guide, not a working demo — never wire it
> to a real client Power BI tenant while developing; use mock/stub implementations behind
> the same interface (per this platform's `demos/` rule).

## Recommended Folder Structure

```text
<ExistingProjectName>/
├── PowerBi/
│   ├── IPowerBiEmbedService.cs        # Contract: GetEmbedInfoAsync(reportId, workspaceId) etc.
│   ├── PowerBiEmbedService.cs         # Real implementation using Microsoft.PowerBI.Api
│   ├── Options/
│   │   └── PowerBiOptions.cs          # TenantId, ClientId, WorkspaceId(s) — bound via IOptions<T>, never hardcoded
│   └── Models/
│       ├── EmbedTokenResponse.cs
│       └── ReportEmbedInfo.cs
├── Controllers/ or Endpoints/
│   └── PowerBiController.cs           # Thin: calls IPowerBiEmbedService, returns embed config to the client
└── wwwroot/js/
    └── powerbi-embed-client.js        # powerbi-client.js wiring on the frontend
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.PowerBI.Api` | Power BI REST API client (reports, embed tokens, datasets) |
| `Microsoft.Identity.Client` (MSAL) | Service principal / app-only auth against Azure AD for the Power BI API |

Frontend: `powerbi-client` (npm) for the embed SDK — not a NuGet package.

## First Things to Configure

1. Confirm the embedding model with the client: "App Owns Data" (service principal, most
   common for embedding into a customer-facing app) vs. "User Owns Data" (each user needs
   their own Power BI Pro/PPU license) — this drives the entire auth setup.
2. Bind `TenantId`, `ClientId`, `ClientSecret`/certificate thumbprint, and workspace/report
   IDs via `IOptions<PowerBiOptions>` — never hardcode, never commit real values.
3. Generate embed tokens server-side only (`GenerateTokenAsync`) — never expose the service
   principal's credentials or a long-lived token to the client/browser.
4. Scope embed tokens to the specific report/dataset and, if using row-level security (RLS),
   the specific user/role — don't issue a token broader than what that user should see.
5. In local/demo development, implement `IPowerBiEmbedService` against a stub/mock returning
   fixture data instead of a real tenant, per this platform's `demos/` rule (§4 of the
   platform `CLAUDE.md`).
6. Set up the paired test project mocking `IPowerBiEmbedService`'s contract before writing
   controller/endpoint logic against it (Test-First).
