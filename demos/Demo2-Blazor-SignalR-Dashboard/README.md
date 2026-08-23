# Demo 2 — Blazor Server + SignalR Live Ops Dashboard

A small, real, runnable "Live Ops Dashboard" that demonstrates this framework's supported
stack for interactive UI + real-time updates: **Blazor Server, SignalR, and Razor component
patterns**, structured so the UI pieces are reusable from a Blazor WebAssembly host too.

No external services, credentials, or connection strings are required — all metrics are
generated in-process by a background service.

> **Note on folder layout.** Unlike Demo1 and Demo3, this demo does not use a top-level
> `src/`/`tests/` split. `SharedComponents` is a Razor Class Library (RCL) meant to be
> referenced directly at the solution root — the standard, idiomatic layout for an RCL meant
> to be shared by multiple hosts (a future Blazor WebAssembly host, in addition to
> `DashboardHost`) — not a structural inconsistency with the other two demos. `DashboardHost`,
> `SharedComponents.Tests`, and `DashboardHost.Tests` sit alongside it at the same level for
> the same reason: this is a small, flat, multi-project RCL solution, not a single deployable
> app with a `src`/`tests` split.

## What it demonstrates

| Framework guidance | Where it's applied here |
|---|---|
| [Blazor Standards](../../wiki/Coding-Standards-Blazor.md) — Server vs. WebAssembly | UI components live in the `SharedComponents` Razor Class Library, not the host, so the same `MetricCard`/`LiveFeed`/`StatusBadge` markup could be referenced from a Blazor WASM host with no changes — only the transport (how data reaches them) would differ. |
| Blazor Standards — `[Parameter]` in, `EventCallback`/public method out | `MetricCard` and `StatusBadge` are pure `[Parameter]`-driven. `LiveFeed` is pushed to imperatively via a public `AddEvent(FeedEvent)` method invoked through a component reference (`@ref`) — the pattern the wiki describes for a component whose state a SignalR callback needs to update. |
| Blazor Standards — `CascadingValue`/`CascadingParameter` for cross-cutting concerns | `MainLayout.razor` wraps the page in `<CascadingValue Value="_theme">`, cascading a `ThemeSettings` record — a cross-cutting, whole-subtree concern, not a parameter threaded through every component. |
| Blazor Standards — dispose anything that owns a SignalR connection | `Dashboard.razor` implements `IAsyncDisposable` and calls `_hubConnection.DisposeAsync()` — required so a disconnected/discarded circuit doesn't leave a dangling hub connection. |
| Blazor Standards — component lifecycle | `Dashboard.razor` starts its hub connection in `OnInitializedAsync` (not the constructor), and registers/unwinds handlers around that lifetime. |
| [SignalR Guidelines](../../wiki/SignalR-Guidelines.md) — strongly-typed hubs over stringly-typed `SendAsync` | `MetricsHub : Hub<IMetricsClient>` and `IHubContext<MetricsHub, IMetricsClient>` are used instead of `Clients.All.SendAsync("MethodName", ...)`, so a mismatched method name/payload is a compile error, not a silent runtime no-op. |
| SignalR Guidelines — hub is a thin RPC surface | `MetricsHub` itself has no logic at all; generation lives in `IMetricsGenerator`, broadcasting lives in `MetricsBroadcastService`. |
| SignalR Guidelines — small message payloads | Only a `MetricSnapshot` (4 fields) or `FeedEvent` (3 fields) is pushed per tick — no large documents over the wire. |
| `EditForm` + validation | `Settings.razor` binds a `DashboardSettings` model with `[Range(2, 60)]`, validated via `DataAnnotationsValidator` + `ValidationSummary`/`ValidationMessage`. |

## Project layout

```
Demo2-Blazor-SignalR-Dashboard/
├── SharedComponents/            Razor Class Library — reusable across Server/WASM hosts
│   ├── Models/                  MetricSnapshot, FeedEvent, ThemeSettings, enums (dependency-free)
│   ├── Services/                IMetricsGenerator / MetricsGenerator (pure simulation logic)
│   ├── MetricCard.razor
│   ├── LiveFeed.razor
│   └── StatusBadge.razor
├── DashboardHost/                Blazor Server host (the "Blazor Web App" .NET 8 template, Server interactivity)
│   ├── Hubs/MetricsHub.cs        Hub<IMetricsClient> — thin, push-only
│   ├── Hubs/IMetricsClient.cs    Strongly-typed client contract
│   ├── Services/MetricsBroadcastService.cs   BackgroundService — ticks, generates, broadcasts
│   ├── Services/FeedEventClassifier.cs       Pure critical/warning/info decision logic (unit-testable)
│   ├── Services/DashboardSettingsService.cs  Singleton holding the current refresh interval
│   ├── Models/DashboardSettings.cs           EditForm-bound, validated settings model
│   └── Components/Pages/
│       ├── Dashboard.razor       /dashboard — connects to the hub, renders live data
│       └── Settings.razor        /settings — change the refresh interval
├── SharedComponents.Tests/       bUnit component tests + xUnit tests for the pure generator logic
└── DashboardHost.Tests/          xUnit tests for DashboardSettingsService, FeedEventClassifier, MetricsHub
```

### Why the host is a "Blazor Web App" template, not the old `blazorserver` template

This machine's cached `dotnet new blazorserver` template turned out to be a stale
ASP.NET Core **3.1** template shadowing the SDK's built-in one (confirmed via
`dotnet new blazorserver -h`, which reports `netcoreapp3.1` as its only supported
target and 3.1-era third-party notices). The task explicitly calls for *current* .NET 8
conventions, not that older template's output, so `DashboardHost` was scaffolded instead
with the current unified template:

```
dotnet new blazor -o DashboardHost -n DashboardHost -int Server -ai true
```

`-int Server` selects Blazor **Server** interactivity (SignalR-based, matching this demo's
scope) and `-ai true` applies it at the top of the render tree, so the app behaves like a
classic all-interactive Blazor Server app while still using the current .NET 8
`Program.cs`/`App.razor` hosting model (`AddRazorComponents().AddInteractiveServerComponents()`,
`MapRazorComponents<App>().AddInteractiveServerRenderMode()`).

## How to run it

Requires the .NET 8 SDK (a `global.json` in this folder pins the SDK version so the demo
builds consistently regardless of other SDKs installed on the machine).

```bash
cd demos/Demo2-Blazor-SignalR-Dashboard
dotnet build
dotnet run --project DashboardHost
```

Then open the printed `https://localhost:xxxx` (or `http://localhost:xxxx`) URL and:

1. Click **Live Dashboard** in the nav (or go to `/dashboard`).
   - Three `MetricCard`s (CPU, Active Users, Error Rate) update automatically every few
     seconds, pushed from the server over SignalR — no manual refresh needed.
   - The `StatusBadge` next to the heading reflects the SignalR connection state (Live/Disconnected).
   - The `LiveFeed` below fills with recent events as they occur (info notes, and
     warning/critical entries if the simulated CPU or error rate spikes).
2. Click **Settings** (or go to `/settings`) and change the refresh interval.
   - Try entering a value outside 2–60 (e.g. `0` or `500`) and submitting — the `EditForm`
     validation blocks it and shows an error message.
   - Enter a valid value (e.g. `2`) and save; go back to **Live Dashboard** and notice the
     metrics now update faster.

## Running the tests

```bash
dotnet test SharedComponents.Tests/SharedComponents.Tests.csproj
dotnet test DashboardHost.Tests/DashboardHost.Tests.csproj
```

Or build/test the whole solution at once:

```bash
dotnet test Demo2-Blazor-SignalR-Dashboard.sln
```

`SharedComponents.Tests` runs:

- **bUnit component tests** — `MetricCardTests`, `LiveFeedTests`, `StatusBadgeTests`:
  render each component with parameters, assert on the rendered markup/CSS classes, and
  (for `LiveFeed`) exercise the imperative `AddEvent` push path and its `MaxItems` trimming.
- **xUnit tests for pure logic** — `MetricsGeneratorTests`: seeds `MetricsGenerator` with a
  fixed `Random` and asserts every generated reading stays within its realistic bounds, that
  a given seed is deterministic, and that consecutive readings never jump by more than the
  generator's configured max step — all without needing a running host, hub, or SignalR
  connection.

`DashboardHost.Tests` runs:

- **`DashboardSettingsServiceTests`** — the settings singleton's `Update`/`Current`
  round-trip correctly, `Current` returns a defensive copy, and concurrent reads/writes
  (via `Parallel.For`) don't throw.
- **`FeedEventClassifierTests`** — the critical/warning/info threshold branching extracted
  from `MetricsBroadcastService` into `FeedEventClassifier.BuildFeedEvent`, covering the
  error-rate/CPU thresholds (including which one wins when both are breached), the random
  info-event roll, and the "nothing to report" case — all deterministic, since the random
  roll is passed in as a parameter rather than generated internally.
- **`MetricsHubTests`** — confirms `/hubs/metrics/negotiate` is mapped and reachable via
  `WebApplicationFactory<Program>`, mirroring Demo1's `TaskHub_NegotiateEndpoint_IsMapped`
  pattern.

## Notes / non-goals

- No authentication/authorization — this is a UI/real-time-transport demo, not a security
  demo. If you were productionizing this, `SignalR-Guidelines.md`'s hub authorization
  section (`[Authorize]` + per-group checks) would apply.
- No backplane — this is a single-instance demo. The SignalR wiki's scale-out guidance
  (Azure SignalR Service or a Redis backplane) applies once you'd run more than one
  instance of `DashboardHost`.
- Metrics are entirely simulated in-process (`MetricsGenerator`); nothing here calls a real
  monitoring API.
