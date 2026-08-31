# Architecture Overview

This page describes how a **typical Dotsquares client solution** is layered, where each supported stack usually sits within that layering, and how the specialized agents in `.claude/agents/` map onto those layers. It is a reference model, not a mandate — individual client projects may deviate, and their own `CLAUDE.md` always takes precedence (see [FAQ](../docs/FAQ.md)).

## The layered model

Most Dotsquares .NET client engagements — regardless of which front-end stack is chosen — converge on the same four-layer shape:

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Presentation                                              │
│    ASP.NET Core MVC / Razor Pages / Blazor Server / Blazor   │
│    WASM / Umbraco (rendered views) / Power Apps canvas apps  │
│    / React / Angular (SPA clients consuming layer 2 as an    │
│    ASP.NET Core Web API)                                      │
├─────────────────────────────────────────────────────────────┤
│ 2. Application / Services                                    │
│    Use-case orchestration, DTOs/view models, validation,     │
│    authorization checks, background jobs, SignalR hubs       │
├─────────────────────────────────────────────────────────────┤
│ 3. Data Access                                                │
│    EF Core DbContexts, repositories, migrations               │
├─────────────────────────────────────────────────────────────┤
│ 4. External Integrations                                      │
│    SQL Server, Microsoft Graph (SharePoint), Power BI         │
│    embed APIs, Dataverse, third-party REST APIs               │
└─────────────────────────────────────────────────────────────┘
```

Requests flow top to bottom; each layer depends only on the layer(s) below it, never sideways into a peer or upward into its caller. This is standard onion/clean-architecture layering — nothing stack-specific about the shape itself, only about what typically occupies each layer.

### Layer 1 — Presentation

This is the layer most affected by stack choice:

- **ASP.NET Core Web API / minimal APIs** — presentation is a set of controllers/endpoint delegates returning DTOs, consumed by an SPA, mobile app, or another service. No server-rendered views.
- **ASP.NET MVC** (legacy, still common on older client codebases) — controllers + Razor `.cshtml` views, server-rendered, full postback model.
- **Razor Pages** — page-per-feature model, `PageModel` co-located with its `.cshtml`; favored for content-oriented or CRUD-heavy pages where full MVC ceremony isn't needed.
- **Blazor Server** — presentation logic runs server-side over a SignalR circuit; UI updates are diffs pushed to the browser. See [SignalR Guidelines](SignalR-Guidelines.md) for the transport this depends on.
- **Blazor WebAssembly** — presentation logic and a trimmed .NET runtime run client-side in the browser sandbox; it talks to layer 2 only through HTTP APIs, never via a shared in-process `DbContext`.
- **Umbraco CMS** — presentation is a hybrid: Umbraco's own rendering pipeline (Razor views bound to content models) for editor-managed pages, plus conventional MVC/API controllers ("surface controllers"/"API controllers") for anything transactional.
- **Power Apps canvas apps** — presentation lives outside the .NET solution entirely, in Power Apps Studio, and talks to layer 2/4 through custom connectors or Dataverse directly.
- **React / Angular** — presentation lives entirely outside the .NET solution, as a separate SPA codebase (its own repo or a sibling folder), talking to layer 2 exclusively over HTTP as an ASP.NET Core Web API — never a shared in-process `DbContext`, the same boundary Blazor WebAssembly already has with the backend. This is the platform's only presentation option with no server-rendered fallback: layer 2 must expose a real API surface, not just support one incidentally. See [Coding Standards — React](Coding-Standards-React.md) / [Coding Standards — Angular](Coding-Standards-Angular.md).

### Layer 2 — Application / Services

Use-case classes, command/query handlers, validators (FluentValidation or DataAnnotations), authorization policies, mapping (AutoMapper or manual), background/queued work (`IHostedService`, Azure Functions, or a message queue), and SignalR hubs for real-time push. This layer is where business rules live — it should be framework-agnostic enough that swapping the presentation layer (e.g., MVC → Blazor) does not require rewriting it.

### Layer 3 — Data Access

EF Core `DbContext`s, entity configurations (`IEntityTypeConfiguration<T>`), migrations, and any repository/unit-of-work abstractions the project has standardized on. See [EF Core Guidelines](EFCore-Guidelines.md) for scoping and migration rules, and [SQL Server Guidelines](SQL-Server-Guidelines.md) for what happens below the ORM.

### Layer 4 — External Integrations

Everything outside the solution's own database: Microsoft Graph for SharePoint/Teams/Exchange data, Power BI REST APIs for embedded analytics, Dataverse/Power Platform connectors, and any third-party REST/SOAP services a client integrates with. These should always sit behind an interface defined in layer 2 (e.g., `ISharePointDocumentService`) so layer 2 never takes a hard dependency on `GraphServiceClient` or a Power BI SDK type directly — this keeps the integration swappable and testable.

## How `.claude/agents` map onto this model

The platform's stack-specific agents are scoped to the layer(s) they're expert in, so invoking the right agent means getting advice grounded in that layer's real constraints instead of generic guidance:

| Layer | Relevant agents |
|---|---|
| Presentation | `aspnet-core-developer`, `mvc-developer`, `razor-pages-developer`, `blazor-developer`, `umbraco-developer`, `powerapps-developer`, `react-developer`, `angular-developer` |
| Application/Services | `aspnet-core-developer` (API/service layer), `signalr-developer` (hubs), cross-cutting `unit-test-writer` |
| Data Access | `efcore-developer` |
| External Integrations | `sql-server-developer`, `sharepoint-developer`, `powerbi-developer`, `powerapps-developer` |
| Cross-cutting (all layers) | `architecture-analyst` (explains flows across layers), `code-reviewer` (reviews a diff against standards), `security-reviewer` (auth/secrets/injection), `build-validator` (final build/test gate) |

When a task spans layers — e.g., "add a document upload feature that stores metadata in SQL Server and the file in SharePoint" — expect to involve more than one agent, or use `architecture-analyst` first to scope which layers and agents are actually touched before implementation starts. This mirrors the [AI Workflow Discipline](AI-Workflow-Discipline.md): understand the blast radius before proposing a change.

## Cross-cutting concerns that don't fit one layer

- **Authentication/authorization** — typically ASP.NET Core Identity, Azure AD/Entra ID, or a client's existing IdP, enforced at layer 1 (attributes/policies) but defined once and reused, not duplicated per controller.
- **Logging/observability** — `ILogger<T>` with structured logging, flowing through all layers; never logging secrets (see [Security Guidelines](../docs/Security-Guidelines.md)).
- **Configuration** — `IOptions<T>` bound from `appsettings.json`/environment/user-secrets, injected wherever needed rather than read ad hoc from `IConfiguration` deep in business logic.
- **CORS** — only relevant once a presentation layer runs on a different origin than the API (React, Angular, and Blazor WebAssembly all qualify): configure an explicit allow-list of origins on the ASP.NET Core API, never a wildcard `AllowAnyOrigin()` alongside credentialed requests — this is a real, not theoretical, security boundary between the SPA and the API.

## Related pages

- [AI Workflow Discipline](AI-Workflow-Discipline.md) — the process discipline applied on top of this architecture.
- [C# Coding Standards](Coding-Standards-CSharp.md) — conventions that apply across every layer.
- [EF Core Guidelines](EFCore-Guidelines.md), [SQL Server Guidelines](SQL-Server-Guidelines.md) — layer 3/4 detail.
