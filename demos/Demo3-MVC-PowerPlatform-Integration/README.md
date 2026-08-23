# Demo 3 — ASP.NET Core MVC + Power Platform / Microsoft 365 Integration

A small "Client Reporting Portal" built with **ASP.NET Core MVC (.NET 8)** that demonstrates the
Dotsquares AI Engineering Platform's integration patterns for:

- **Power BI embedded analytics** (service-principal "app owns data" embedding)
- **SharePoint / Microsoft Graph documents**
- **A Power Apps custom-connector-shaped API surface**

Every integration is built against a clean interface with a **mock implementation behind it**, so
the whole app runs end-to-end, in about ten seconds, with **zero real Azure tenant, zero
credentials, and zero network calls to Microsoft services**. The point of the demo is the seam:
every mock is written so a developer can see exactly what a real implementation would need to
change, without touching a single controller or view.

## What this demonstrates

| Page / route | Interface | Mock implementation | What it demonstrates |
|---|---|---|---|
| `/Dashboard` | `IPowerBiEmbedService` | `MockPowerBiEmbedService` | Power BI REST "GenerateToken" embed flow + the real `powerbi-client` JS SDK wired to consume it |
| `/Documents` | `ISharePointDocumentService` | `MockSharePointDocumentService` | Microsoft Graph document library list/upload/download-link shape |
| `/Admin` | (reads both of the above + `ITaskService`) | — | Shows which implementation each seam is currently wired to, straight from DI |
| `/api/tasks` (+ `/swagger`) | `ITaskService` | `InMemoryTaskService` | A CRUD API shaped for import into Power Apps as a custom connector |

## The "mock now / real later" seam

Each integration follows the same pattern:

1. An interface in `src/ClientReportingPortal.Web/Contracts/<Area>/` whose method signatures and
   DTOs mirror the **real** SDK/REST shape (Power BI REST `GenerateToken`, Graph `DriveItem`,
   etc.) - not a simplified stand-in.
2. A `Mock<Area>Service` in `src/ClientReportingPortal.Web/Services/<Area>/` that implements the
   interface with realistic fake data and **no network access**.
3. An XML doc comment on the mock, titled "What Real&lt;Area&gt;Service would do differently",
   spelling out the auth flow, SDK, and NuGet packages a production implementation would use.
4. A single DI registration in `Program.cs` binding the interface to the mock. Swapping to a real
   implementation is a **one-line change** in `Program.cs` - no controller or view changes.

### Power BI (`IPowerBiEmbedService` / `MockPowerBiEmbedService`)

- Real shape: `GetEmbedTokenAsync(reportId, workspaceId, effectiveIdentity)` → `EmbedConfig`
  (`embedUrl`, `embedToken`, `reportId`, expiry), matching Power BI's REST
  `POST /v1.0/myorg/groups/{workspaceId}/reports/{reportId}/GenerateToken` response.
- `RealPowerBiEmbedService` would additionally need:
  - **Auth**: `Azure.Identity.ClientSecretCredential` (service-principal / "app owns data" embedding).
  - **API call**: `Microsoft.PowerBI.Api`'s `PowerBIClient` (or a raw HTTP POST) against the
    `GenerateToken` endpoint, passing an `IdentityBlob` built from `EffectiveIdentity` so Power BI
    applies row-level security (RLS) for the signed-in portal user.
  - **Caching**: embed tokens are short-lived (~1 hour); cache per `(reportId, effectiveIdentity)`.
- The `/Dashboard` view includes the real `powerbi-client` JS embed snippet
  (`powerbi.embed(container, embedConfiguration)`), loaded from a CDN and fed by the mock's
  `EmbedConfig`. Because the token/URL are fake, the iframe itself will show a Power BI error - the
  JavaScript wiring is real even though the credentials are not.

### SharePoint (`ISharePointDocumentService` / `MockSharePointDocumentService`)

- `DocumentsController.Upload` enforces a basic guardrail before calling the service: a 10 MB
  max file size and an extension allowlist (`.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`,
  `.pptx`, `.txt`, `.csv`). Either failing returns a `400` `ProblemDetails` response. A real,
  non-mock `ISharePointDocumentService` must re-apply the same checks server-side before
  forwarding bytes to Microsoft Graph - client-declared size/extension are not trustworthy
  on their own.
- Real shape: `ListDocumentsAsync(siteId, driveId)`, `UploadDocumentAsync(...)`,
  `GetDocumentDownloadUrlAsync(...)`, matching Microsoft Graph's
  `GET /sites/{siteId}/drives/{driveId}/root/children`, `PUT .../root:/{name}:/content`, and the
  `@microsoft.graph.downloadUrl` facet respectively.
- `RealSharePointDocumentService` would additionally need:
  - **SDK**: `Microsoft.Graph` (`GraphServiceClient`) instead of hand-rolled HTTP.
  - **Auth**: app-only (client-credentials) auth via `Microsoft.Identity.Client` or
    `Azure.Identity.ClientSecretCredential` - no signed-in user needed for a background reporting
    portal.
  - **Scopes**: least privilege - `Sites.Read.All` (add `Sites.ReadWrite.All` only if upload is
    required), narrowed further with a Graph application access policy where possible.
  - **Resilience**: a Polly retry policy honoring Graph's `Retry-After` header for HTTP 429/503
    throttling.
  - **Paging**: follow `@odata.nextLink` for libraries larger than one page.

### Power Apps custom connector (`/api/tasks`)

`Controllers/Api/TasksController.cs` is a plain CRUD API (`GET/POST/PUT/DELETE /api/tasks`) with
typed request/response DTOs and Swashbuckle-generated OpenAPI (`/swagger/v1/swagger.json`,
browsable UI at `/swagger`). Swagger is only mapped when `IsDevelopment()` is true (matching
Demo1's pattern) - it is not exposed if this app is ever run with a non-Development
environment.

**How this becomes a Power Apps custom connector:**

1. In Power Apps (or the Power Platform admin center), choose **Data → Custom connectors → New
   custom connector → Import an OpenAPI file**, and point it at this API's
   `/swagger/v1/swagger.json`.
2. Set the connector's **host** to the deployed API's base URL and choose an **auth type** - for a
   real deployment this would typically be **API Key** (a header checked by middleware) or
   **OAuth 2.0** (Azure AD, matching how `MFAApi`/`IntegrationsWebhook` in the main framework
   authenticate); this demo has no auth middleware since it never leaves localhost.
3. Power Apps generates one action per operation (`GetAll`, `GetById`, `Create`, `Update`,
   `Delete`) that a canvas app or Power Automate flow can call directly.
4. **Dataverse alternative**: for a production Power Apps solution, prefer a native Dataverse
   table over a custom connector when the app owns the data model - it gets built-in security
   roles, offline sync, and Power Automate triggers for free. A custom connector (this API) makes
   more sense when the data already lives in an existing system of record that Dataverse
   shouldn't duplicate - which is the scenario this demo represents.

## Project layout

```
Demo3-MVC-PowerPlatform-Integration/
├── Demo3-MVC-PowerPlatform-Integration.sln
├── global.json                              # pins the .NET 8 SDK
├── src/ClientReportingPortal.Web/            # the MVC app
│   ├── Contracts/{PowerBi,SharePoint,Tasks}/ # interfaces + DTOs (the "real" shape)
│   ├── Services/{PowerBi,SharePoint,Tasks}/  # mock implementations
│   ├── Controllers/                          # Dashboard, Documents, Admin, Home (MVC)
│   ├── Controllers/Api/TasksController.cs    # Power-Apps-connector-shaped API
│   ├── Views/{Dashboard,Documents,Admin,Home}/
│   └── Program.cs                            # DI wiring + Swagger setup
└── tests/ClientReportingPortal.Tests/        # xUnit + Moq + WebApplicationFactory
```

## Prerequisites

- .NET 8 SDK (see `global.json`; the repo's other demos use the same pin).
- No Azure subscription, Power BI tenant, SharePoint site, or Power Apps environment required.

## How to run

```bash
dotnet restore Demo3-MVC-PowerPlatform-Integration.sln
dotnet build Demo3-MVC-PowerPlatform-Integration.sln
dotnet run --project src/ClientReportingPortal.Web/ClientReportingPortal.Web.csproj
```

Then open the URL printed in the console (typically `http://localhost:5125`) and visit:

- `/` - landing page with links to each demo page
- `/Dashboard` - Power BI embed
- `/Documents` - SharePoint document list/upload
- `/Admin` - integration status overview
- `/swagger` - the Tasks API's OpenAPI UI

## How to test

```bash
dotnet test tests/ClientReportingPortal.Tests/ClientReportingPortal.Tests.csproj
```

The suite covers:

- `MockPowerBiEmbedServiceTests` - embed config shape, argument validation, token expiry.
- `MockSharePointDocumentServiceTests` - listing, upload, download-link resolution and its
  not-found case, argument validation.
- `InMemoryTaskServiceTests` - full CRUD behavior.
- `TasksControllerTests` - each API action against a mocked `ITaskService` (Moq), including the
  model-validation and not-found paths.
- `DashboardControllerTests` / `DocumentsControllerTests` - MVC controllers unit-tested with
  mocked service interfaces (Moq), asserting the correct view/model or redirect.
- `WebAppIntegrationTests` - boots the real app via `WebApplicationFactory<Program>` (all mocks
  included) and hits `/`, `/Dashboard`, `/Documents`, `/Admin`, `/api/tasks`, and
  `/swagger/v1/swagger.json` over an in-memory HTTP client.

No test requires network access or external credentials.

## What you'd add for a real deployment

| Concern | NuGet package(s) | Notes |
|---|---|---|
| Power BI embed auth + REST calls | `Azure.Identity`, `Microsoft.PowerBI.Api` | Service-principal "app owns data" embedding; see `MockPowerBiEmbedService.cs` doc comment. |
| SharePoint/Graph document access | `Microsoft.Graph`, `Microsoft.Identity.Client` | App-only auth, least-privilege scopes, Polly retry; see `MockSharePointDocumentService.cs` doc comment. |
| Resilience for Graph throttling | `Polly` | Wrap Graph calls; honor `Retry-After`. |
| Real task storage | (already-used EF Core / SQL, or Dataverse) | Replace `InMemoryTaskService` behind the existing `ITaskService` interface. |
| Azure resources needed | Azure AD app registration (client secret or certificate), Power BI workspace + service-principal access, SharePoint site with the app granted `Sites.Read.All`/`Sites.ReadWrite.All`, a Power Apps environment for the custom connector | None of these are required to run this demo - only to swap in the real implementations. |

No real tenant IDs, client IDs, or secrets appear anywhere in this demo - configuration
placeholders use `<TENANT_ID>`-style tokens or the all-zero GUID
`00000000-0000-0000-0000-000000000000` (see `appsettings.json`).
