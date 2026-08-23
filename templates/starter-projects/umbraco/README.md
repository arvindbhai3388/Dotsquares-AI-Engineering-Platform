# Starter Scaffold — Umbraco CMS

> Template outline for bootstrapping a new Umbraco CMS solution. This is a
> folder-structure and setup guide, not a working demo — see `demos/` for a runnable example.

## Recommended Folder Structure

```text
<ProjectName>/
├── <ProjectName>.csproj
├── Program.cs                        # AddUmbraco().Build()/StartWithOptions()
├── appsettings.json                  # Shape only — Umbraco:CMS connection string etc. as placeholders
├── umbraco/                          # Umbraco backoffice + generated files — treat as generated
├── App_Plugins/                      # Custom backoffice property editors / dashboards
│   └── <CustomPackage>/
├── Views/
│   ├── <DocTypeAlias>.cshtml         # Views named after document type aliases, by convention
│   └── Partials/
├── Models/
│   ├── Generated/                    # ModelsBuilder output — do not hand-edit, treat as generated
│   └── ViewModels/                   # Hand-written composition/view models, if used
├── Composers/
│   └── <Feature>Composer.cs          # IComposer implementations for custom DI/pipeline wiring
├── Controllers/
│   ├── <Feature>Controller.cs        # Surface controllers, if adding custom API endpoints
│   └── Render<DocType>Controller.cs  # Custom render controllers overriding default rendering
└── wwwroot/
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Umbraco.Cms` | Core CMS package (pulls in backoffice, Examine, etc.) |
| `Umbraco.Cms.Persistence.SqlServer` (or the client's DB provider package) | Database provider |
| `Umbraco.Cms.ModelsBuilder` (built in, configure mode) | Strongly typed models for document types |

Do not add unrelated Umbraco Marketplace packages without checking they're actually needed
for this client's requirements — each one is a long-term upgrade/compatibility liability.

## First Things to Configure

1. Confirm the Umbraco version and target .NET version compatibility (check
   `wiki/`-style compatibility notes if present) before scaffolding.
2. Set ModelsBuilder mode (`InMemoryAuto` for dev convenience vs. `SourceCodeManual`/
   `SourceCodeAuto` for production builds where generated models are checked in) — pick one
   convention project-wide.
3. Never hand-edit generated ModelsBuilder classes — extend via partial classes instead.
4. Set up document type → template → view naming convention early (alias casing, partial
   view organization) and keep it consistent.
5. Treat `appsettings.json`'s `ConnectionStrings` and `Umbraco:CMS` sections as restricted —
   document placeholder shapes only, never commit real connection strings or license keys.
6. Set up the paired test project for any custom composers/controllers/services (xUnit,
   matching the platform default) before writing them (Test-First) — Umbraco's own core is
   not something this project tests, only the client's customizations are.
