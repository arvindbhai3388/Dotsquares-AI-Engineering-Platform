# Starter Scaffold — Razor Pages

> Template outline for bootstrapping a new ASP.NET Core Razor Pages application. This is a
> folder-structure and setup guide, not a working demo — see `demos/` for a runnable example.

## Recommended Folder Structure

```text
<ProjectName>/
├── <ProjectName>.csproj
├── Program.cs
├── appsettings.json                  # Shape only — no real values committed
├── Pages/
│   ├── <Feature>/
│   │   ├── Index.cshtml
│   │   ├── Index.cshtml.cs           # PageModel — keep handlers thin
│   │   ├── Create.cshtml
│   │   └── Create.cshtml.cs
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── Services/
│   ├── I<Feature>Service.cs
│   └── <Feature>Service.cs
├── Models/
│   └── Domain/
├── Options/
│   └── <Feature>Options.cs
└── wwwroot/
    ├── css/
    ├── js/
    └── lib/
```

Keep folder-per-feature under `Pages/` rather than flattening everything — mirrors routing
and keeps `PageModel`s next to their markup.

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.AspNetCore.Mvc.Testing` | Integration tests via `WebApplicationFactory` |
| `FluentValidation.AspNetCore` | Validation beyond data annotations, if already a project convention |
| `Microsoft.Extensions.Options.DataAnnotations` | Options validation |

## First Things to Configure

1. Decide the routing convention for `Pages/` (folder-based default vs. `@page` route
   overrides) and keep it consistent project-wide.
2. Keep `PageModel.OnGet`/`OnPost` handlers thin — push logic into injected services, same
   discipline as thin MVC controllers.
3. Set up anti-forgery token validation on all `OnPost` handlers that mutate state (enabled
   by default via the Razor Pages tag helpers — don't disable it).
4. Wire up `IOptions<T>` + validation for configuration sections before writing business
   logic against them.
5. Set up the paired test project (xUnit + `WebApplicationFactory`, matching the platform's
   default) before writing the first page handler (Test-First).
6. Never commit real values into `appsettings.json` — document placeholder shapes only.
