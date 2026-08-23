# Starter Scaffold — Blazor WebAssembly

> Template outline for bootstrapping a new Blazor WebAssembly application (standalone or
> hosted on an ASP.NET Core backend). This is a folder-structure and setup guide, not a
> working demo — see `demos/` for a runnable example.

## Recommended Folder Structure

```text
<ProjectName>.Client/                 # The WASM app itself
├── <ProjectName>.Client.csproj
├── Program.cs                        # WebAssemblyHostBuilder, DI registrations
├── wwwroot/
│   ├── index.html
│   ├── appsettings.json              # Shape only — client-side config is public by nature
│   └── css/ , js/
├── App.razor
├── Layout/
│   ├── MainLayout.razor
│   └── NavMenu.razor
├── Pages/
│   └── <Feature>/
│       └── <Feature>Index.razor
├── Services/
│   ├── I<Feature>ApiClient.cs        # Typed HttpClient wrapper calling the backend API
│   └── <Feature>ApiClient.cs
└── _Imports.razor

<ProjectName>.Server/                 # Only if "hosted" model — the ASP.NET Core backend API
└── (see aspnet-core starter-project README)

<ProjectName>.Shared/                 # Only if hosted — DTOs shared between Client and Server
└── Dtos/
```

**Never treat any file under `wwwroot/` as a place for secrets** — everything shipped to the
WASM client is downloadable and inspectable by any browser user, including `appsettings.json`.

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.AspNetCore.Components.WebAssembly` | Core WASM hosting |
| `Microsoft.AspNetCore.Components.WebAssembly.Authentication` | OIDC/token-based auth against the backend, if required |
| `Microsoft.Extensions.Http` | Typed `HttpClient` registration (`AddHttpClient<T>`) for API calls |
| `bunit` | Component unit testing |

## First Things to Configure

1. Decide standalone vs. hosted model up front — hosted requires a `.Server` API project and
   a `.Shared` DTO project; standalone talks to an externally hosted API directly.
2. Register a typed `HttpClient` (via `AddHttpClient<TClient>`) pointed at the backend API's
   base address — never hardcode the API URL; bind it from `wwwroot/appsettings.json`.
3. Never put secrets, API keys, or connection strings anywhere under `wwwroot/` — the WASM
   bundle and all its assets are fully client-downloadable. Authenticate via a token flow
   (OIDC/`AuthenticationStateProvider`) against the backend instead.
4. If hosted, keep authorization checks on the `.Server` API — the WASM client's UI-level
   checks (`[Authorize]` on a page) are UX only, not a security boundary.
5. Set up the paired test project (xUnit + `bunit`) before writing the first non-trivial
   component (Test-First).
