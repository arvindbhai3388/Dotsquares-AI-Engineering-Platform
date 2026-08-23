# Starter Scaffold — Blazor Server

> Template outline for bootstrapping a new Blazor Server application. This is a
> folder-structure and setup guide, not a working demo — see `demos/` for a runnable example.

## Recommended Folder Structure

```text
<ProjectName>/
├── <ProjectName>.csproj
├── Program.cs                        # AddServerSideBlazor(), circuit options
├── appsettings.json                  # Shape only — no real values committed
├── App.razor
├── Components/                       # Or "Shared/" per project convention — pick one
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── <Feature>/
│       └── <Feature>Card.razor       # Small, reusable, single-purpose components
├── Pages/
│   └── <Feature>/
│       └── <Feature>Index.razor      # Routable pages (@page directive)
├── Services/
│   ├── I<Feature>Service.cs
│   └── <Feature>Service.cs           # Scoped per-circuit, not singleton, unless stateless
├── State/
│   └── <Feature>State.cs             # Cascading/scoped state container, if not using a library
├── wwwroot/
│   ├── css/
│   └── js/
│       └── interop.js                # JS interop — keep isolated, not scattered inline
└── _Imports.razor
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.AspNetCore.Components.Web` | Core Blazor component model |
| `Fluxor` or similar (only if the client project needs centralized state) | State management beyond cascading params/scoped services |
| `Microsoft.AspNetCore.SignalR.Client` (implicit via Blazor Server) | Underlying circuit transport — rarely referenced directly |
| `bunit` | Component unit testing |

Don't add a state-management library unless cascading parameters and scoped DI services are
genuinely insufficient — most CRUD-style features don't need one.

## First Things to Configure

1. Set circuit options (`AddServerSideBlazor(options => ...)`) for
   `DisconnectedCircuitMaxRetained`, `MaxBufferedUnacknowledgedRenderBatches`, and
   `DetailedErrors` (dev only) deliberately, not left at framework defaults without review.
2. Register feature services as `Scoped` (per-circuit) by default — a `Singleton` service
   shared across all users' circuits is a common source of cross-user data leaks in Blazor
   Server.
3. Isolate JS interop calls behind a thin wrapper service rather than calling
   `IJSRuntime.InvokeAsync` directly from components.
4. Decide the component folder convention (`Components/` vs `Shared/`) up front and apply it
   consistently.
5. Set up the paired test project (xUnit + `bunit`) before writing the first non-trivial
   component (Test-First).
6. Never commit real values into `appsettings.json` — document placeholder shapes only.
