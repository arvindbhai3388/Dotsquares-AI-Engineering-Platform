# Starter Scaffold — ASP.NET Core (Web API / Minimal APIs)

> Template outline for bootstrapping a new ASP.NET Core Web API on a client project. This is
> a folder-structure and setup guide, not a working demo — see `demos/` for a runnable
> example. Copy the structure, then fill in real code per the client's actual requirements.

## Recommended Folder Structure

```text
<ProjectName>/
├── <ProjectName>.csproj
├── Program.cs                      # Minimal hosting model: builder, DI, middleware pipeline
├── appsettings.json                 # Shape only — no real values committed
├── appsettings.Development.json     # Gitignored — local dev secrets/overrides
├── Endpoints/                       # If using minimal APIs: one static class per feature group
│   └── <Feature>Endpoints.cs
├── Controllers/                     # If using controllers instead of minimal APIs
│   └── <Feature>Controller.cs
├── Services/
│   ├── I<Feature>Service.cs
│   └── <Feature>Service.cs
├── Models/
│   ├── Requests/
│   └── Responses/
├── Options/
│   └── <Feature>Options.cs          # Bound via IOptions<T>, validated on start
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs  # Or IExceptionHandler (net8+)
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # One AddXyz() per feature/cross-cutting concern
└── <ProjectName>.http                # Optional: manual request scratch file
```

Pick **either** `Controllers/` **or** `Endpoints/` — don't mix both patterns in one project.

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Swashbuckle.AspNetCore` or `Microsoft.AspNetCore.OpenApi` | OpenAPI/Swagger docs |
| `Microsoft.Extensions.Options.DataAnnotations` | Options validation |
| `Serilog.AspNetCore` (or the client's existing logging stack) | Structured logging |
| `FluentValidation.AspNetCore` | Request validation, if project rules exceed data annotations |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` for integration tests |

Only add packages actually needed — don't pre-install the whole list.

## First Things to Configure

1. Set the target framework (`<TargetFramework>net8.0</TargetFramework>` or the client's
   required LTS version) in the `.csproj`.
2. Decide controllers vs. minimal APIs and note the decision in the project's `CLAUDE.md`
   §4.1.
3. Wire up `IOptions<T>` + `.ValidateDataAnnotations().ValidateOnStart()` for every
   configuration section before writing business logic against it.
4. Add exception-handling middleware / `IExceptionHandler` returning `ProblemDetails` before
   the first real endpoint is written.
5. Set up the paired test project (`<ProjectName>.Tests`, xUnit + Moq +
   `Microsoft.AspNetCore.Mvc.Testing`) before writing the first endpoint (Test-First).
6. Confirm CORS, HTTPS redirection, and authentication scheme requirements with the client
   before scaffolding is considered "done."
7. Never commit real values into `appsettings.json` — document placeholder shapes only.
