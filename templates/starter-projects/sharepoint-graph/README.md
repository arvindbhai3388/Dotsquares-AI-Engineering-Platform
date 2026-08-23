# Starter Scaffold — SharePoint Integration via Microsoft Graph

> Template outline for bootstrapping SharePoint integration (sites, lists, document
> libraries) via the Microsoft Graph SDK inside an ASP.NET Core application. This is a
> folder-structure and setup guide, not a working demo — never wire it to a real client
> tenant while developing; use mock/stub implementations behind the same interface (per this
> platform's `demos/` rule).

## Recommended Folder Structure

```text
<ExistingProjectName>/
├── SharePoint/
│   ├── ISharePointService.cs          # Contract: GetListItemsAsync, UploadDocumentAsync, etc.
│   ├── SharePointService.cs           # Real implementation using Microsoft.Graph GraphServiceClient
│   ├── Options/
│   │   └── SharePointOptions.cs       # TenantId, ClientId, SiteId — bound via IOptions<T>, never hardcoded
│   └── Models/
│       ├── SiteListItemDto.cs
│       └── DocumentUploadResult.cs
├── Controllers/ or Endpoints/
│   └── SharePointController.cs        # Thin: calls ISharePointService only
└── Auth/
    └── GraphClientFactory.cs          # Builds GraphServiceClient with the chosen credential type
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.Graph` | Microsoft Graph SDK (sites, lists, drives/documents) |
| `Azure.Identity` | Credential types (`ClientSecretCredential`, `ClientCertificateCredential`, `DefaultAzureCredential`) for Graph auth |

## First Things to Configure

1. Confirm the auth approach with the client: app-only (client credentials, for
   background/service-to-service access to a whole site) vs. delegated (acting as the signed-
   in user, respecting that user's own SharePoint permissions) — this drives which Graph
   permissions to request and how tokens are acquired.
2. Bind `TenantId`, `ClientId`, `ClientSecret`/certificate, and target site/list/library IDs
   via `IOptions<SharePointOptions>` — never hardcode, never commit real values.
3. Request the minimum Graph permission scope needed (e.g. `Sites.Selected` scoped to one
   site, rather than tenant-wide `Sites.ReadWrite.All`) unless the client explicitly needs
   broader access.
4. Wrap `GraphServiceClient` calls behind `ISharePointService` so callers never depend on the
   Graph SDK's types directly — makes it swappable and mockable.
5. Handle Graph throttling (`429 Too Many Requests` with `Retry-After`) — use the SDK's
   built-in retry handler or Polly rather than failing hard on the first throttle response.
6. In local/demo development, implement `ISharePointService` against a stub/mock returning
   fixture data instead of a real tenant, per this platform's `demos/` rule (§4 of the
   platform `CLAUDE.md`).
7. Set up the paired test project mocking `ISharePointService`'s contract before writing
   controller/endpoint logic against it (Test-First).
