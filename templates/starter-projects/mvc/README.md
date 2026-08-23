# Starter Scaffold — ASP.NET MVC

> Template outline for bootstrapping a new ASP.NET MVC application (classic .NET Framework
> or ASP.NET Core MVC). This is a folder-structure and setup guide, not a working demo — see
> `demos/` for a runnable example.

## Recommended Folder Structure

```text
<ProjectName>/
├── <ProjectName>.csproj
├── Program.cs / Global.asax.cs      # ASP.NET Core MVC vs. classic .NET Framework
├── Startup.cs                       # ASP.NET Core MVC only
├── Web.config                       # Classic .NET Framework only — never commit real values
├── appsettings.json                 # ASP.NET Core MVC only — shape only, no real values
├── Controllers/
│   └── <Feature>Controller.cs
├── Models/
│   ├── ViewModels/
│   └── Domain/
├── Views/
│   ├── <Feature>/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   └── _ViewStart.cshtml
├── Services/
│   ├── I<Feature>Service.cs
│   └── <Feature>Service.cs
├── Filters/
│   └── <Custom>ActionFilterAttribute.cs
├── App_Start/                       # Classic .NET Framework only: RouteConfig, BundleConfig, FilterConfig
└── wwwroot/ or Content/ + Scripts/  # Static assets — ASP.NET Core vs. classic naming
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.AspNetCore.Mvc.*` (Core) or built-in `System.Web.Mvc` (Framework) | MVC framework itself |
| `AutoMapper` (or `AutoMapper.Extensions.Microsoft.DependencyInjection`) | Domain ↔ ViewModel mapping, if the client already uses it |
| `FluentValidation.AspNetCore` / `FluentValidation.Mvc5` | Server-side validation beyond data annotations |
| `Microsoft.AspNetCore.Mvc.Testing` (Core) | Integration tests via `WebApplicationFactory` |

Match whatever mapping/validation library the client project already has — don't introduce a
second one.

## First Things to Configure

1. Confirm classic .NET Framework MVC 5 vs. ASP.NET Core MVC — folder layout, `Web.config`
   vs. `appsettings.json`, and DI container differ significantly between them.
2. Set up routing conventions (attribute routing vs. conventional `{controller}/{action}/{id}`)
   and note the decision.
3. Establish the ViewModel convention early — controllers should not pass domain/EF entities
   directly to views.
4. Wire up model validation (data annotations + `ModelState.IsValid` checks, or
   `[ApiController]`-style auto-validation is not available in classic MVC).
5. Set up the paired test project (MSTest or xUnit, matching the client's existing
   convention) before writing the first controller action (Test-First).
6. Never commit real values into `Web.config`/`appsettings.json` — document placeholder
   shapes only, and treat both as restricted per the platform's config rule.
