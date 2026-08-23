# Starter Scaffold — Power Apps / Power Platform Connector

> Template outline for bootstrapping a custom connector (backing API + connector
> definition) that Power Apps/Power Automate consumes, or a Dataverse plugin. This is a
> folder-structure and setup guide, not a working demo — never wire it to a real client
> Power Platform environment while developing; use mock/stub implementations behind the same
> interface (per this platform's `demos/` rule).

## Recommended Folder Structure

```text
<ConnectorName>.Api/                  # The backing REST API the custom connector calls
├── (see aspnet-core starter-project README for the API project's own structure)
└── Controllers/ or Endpoints/
    └── <Feature>Controller.cs         # Endpoints exposed to Power Apps/Power Automate

<ConnectorName>.Connector/             # Custom connector definition (not a .csproj — connector metadata)
├── apiDefinition.swagger.json         # OpenAPI 2.0 definition Power Platform imports
├── apiProperties.json                 # Connector auth type, icon, host — placeholders for tenant-specific values
└── settings.json                      # paconn/pac CLI deployment settings — placeholders only

<PluginName>.Plugins/                  # Only if building a Dataverse plugin instead of/alongside a connector
├── <PluginName>.Plugins.csproj        # Must target the Dataverse plugin-compatible profile
├── Plugins/
│   └── <Entity><Event>Plugin.cs       # One plugin class per entity+message (Create/Update/Delete)
└── Helpers/
    └── PluginExecutionContextExtensions.cs
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.PowerPlatform.Dataverse.Client` | Dataverse Web API client (`ServiceClient`), for API-side code calling Dataverse |
| `Microsoft.CrmSdk.CoreAssemblies` | Required references for Dataverse plugin projects (`IPlugin`, `IPluginExecutionContext`) |
| `paconn-cli` (Python/npm tool, not NuGet) or `pac connector` (Power Platform CLI) | Custom connector packaging/deployment |

## First Things to Configure

1. Decide custom connector (calls an externally hosted API) vs. Dataverse plugin (runs
   inside Dataverse's own pipeline) vs. Power Automate flow — each has a different deployment
   and testing story; confirm with the client before scaffolding.
2. For a custom connector: define auth in `apiProperties.json` (API key, OAuth2, AAD) — never
   commit real client secrets/tenant IDs into the connector definition; keep placeholders and
   supply real values only at import/deploy time via the target environment.
3. For a Dataverse plugin: keep plugins **synchronous and fast** for pre-operation logic;
   move slow/non-critical work to async plugins or a separate background service — a slow
   synchronous plugin blocks the user's save operation.
4. Never throw unhandled exceptions from a plugin without wrapping them in
   `InvalidPluginExecutionException` with a user-meaningful message — an unhandled exception
   surfaces as a generic, unhelpful error in the Power Apps UI.
5. Version the plugin assembly and register it via a documented deployment step (Plugin
   Registration Tool or `pac plugin push`) — don't hand-register without recording the step
   for other developers.
6. In local/demo development, mock the Dataverse/connector boundary behind an interface
   rather than hitting a real Power Platform environment, per this platform's `demos/` rule
   (§4 of the platform `CLAUDE.md`).
7. Set up the paired test project before writing plugin/connector logic (Test-First) — plugin
   logic should be extracted into a testable service, not left only reachable via the live
   Dataverse pipeline.
