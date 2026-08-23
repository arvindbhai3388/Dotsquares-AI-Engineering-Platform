# Dotsquares AI Engineering Platform — Wiki

This is the index for the Dotsquares AI Engineering Platform wiki: architecture guidance, per-stack coding standards, and integration guides for teams using Claude Code across Dotsquares .NET client projects. If you are brand new here, start with [Onboarding Guide](Onboarding-Guide.md) and [docs/Getting-Started.md](../docs/Getting-Started.md).

## Foundations

| Page | What it covers |
|---|---|
| [Architecture Overview](Architecture-Overview.md) | How a typical Dotsquares client solution is layered, where each supported stack sits, and how `.claude/agents` map onto those layers. |
| [AI Workflow Discipline](AI-Workflow-Discipline.md) | The analyze → propose → approve → implement → test → review discipline: why each gate exists, realistic failure modes when it's skipped, and how `.claude/skills` enforce it. |
| [Onboarding Guide](Onboarding-Guide.md) | A new developer's first two weeks: what to read, what to run, how to pick the right agent/skill for a task. |

## Coding Standards

| Page | What it covers |
|---|---|
| [C# Coding Standards](Coding-Standards-CSharp.md) | General C#/.NET conventions: naming, nullable reference types, async/await rules, DI lifetimes, exception handling policy. |
| [ASP.NET Core, MVC & Razor Pages Standards](Coding-Standards-AspNetCore-MVC-Razor.md) | Thin controllers, view-model separation, model validation, API versioning, `ProblemDetails` error responses. |
| [Blazor Standards](Coding-Standards-Blazor.md) | Component design, state management patterns, Server vs. WebAssembly decision criteria, JS interop rules. |

## Stack & Integration Guides

| Page | What it covers |
|---|---|
| [Umbraco Guidelines](Umbraco-Guidelines.md) | Content modeling, custom property editors, safe upgrade practices, output caching, content-picker pitfalls. |
| [EF Core Guidelines](EFCore-Guidelines.md) | Expand/contract migration strategy, query performance rules, `DbContext` scoping, concurrency handling. |
| [SQL Server Guidelines](SQL-Server-Guidelines.md) | Indexing strategy, parameterization/injection prevention, stored procedure vs. inline query guidance, execution plan review. |
| [SignalR Guidelines](SignalR-Guidelines.md) | Hub design, groups vs. users, scaling with a backplane (Azure SignalR Service / Redis), hub authentication/authorization. |
| [Power BI Integration](PowerBI-Integration.md) | Embedded analytics architecture, service principal auth, embed token flow, row-level security, workspace/capacity planning. |
| [SharePoint Integration](SharePoint-Integration.md) | Microsoft Graph auth models (app-only vs. delegated), throttling/retry patterns, common site/list/drive operations, CSOM vs. Graph. |
| [Power Apps Integration](PowerApps-Integration.md) | Custom connector design, Dataverse integration patterns, canvas vs. model-driven backend considerations. |

## Demo Projects

Runnable, independently-buildable examples that exercise this framework's patterns end to end (all build clean and pass their test suites — see [`docs/PRODUCTION-READINESS-AUDIT.md`](../docs/PRODUCTION-READINESS-AUDIT.md) for the latest verification):

| Demo | What it demonstrates |
|---|---|
| [Demo 1 — ASP.NET Core + EF Core API](../demos/Demo1-AspNetCore-EFCore-API/README.md) | Web API + EF Core Code-First + SignalR notifications on a small task-tracker domain. |
| [Demo 2 — Blazor + SignalR Dashboard](../demos/Demo2-Blazor-SignalR-Dashboard/README.md) | Blazor Server + a shared Razor Class Library + a live SignalR-driven metrics dashboard. |
| [Demo 3 — MVC + Power Platform Integration](../demos/Demo3-MVC-PowerPlatform-Integration/README.md) | ASP.NET Core MVC with mock-now/real-later integration seams for Power BI, SharePoint, and a Power Apps-connector-shaped API. |

## See also — `docs/`

The `docs/` folder covers process and setup rather than technical standards:

| Page | What it covers |
|---|---|
| [Getting Started](../docs/Getting-Started.md) | Prerequisites, cloning a client project, dropping in the right `templates/CLAUDE.md`, first Claude Code session walkthrough. |
| [Claude Code Setup](../docs/Claude-Code-Setup.md) | Installing Claude Code, `.claude/settings.json` basics, the permissions model, how agents/skills are discovered. |
| [MCP Setup](../docs/MCP-Setup.md) | What MCP is in this platform's context, why credentials stay per-client-project, and how to adapt `templates/mcp-baseline.json` for a new client repo. |
| [Security Guidelines](../docs/Security-Guidelines.md) | Secrets handling, least-privilege scopes for Graph/Power Platform, the restricted-files pattern for client projects. |
| [FAQ](../docs/FAQ.md) | Common questions about scope, ownership of AI output, and adapting the framework to a client's existing conventions. |

## How to use this wiki

- Every page here assumes you have already read the root [`.claude/CLAUDE.md`](../.claude/CLAUDE.md) for this repository — it is the source of truth for repo-wide rules and takes precedence over anything below if they ever disagree.
- Wiki pages describe **standards and rationale**. Day-to-day workflow mechanics (which agent to invoke, which skill to run) live in `.claude/agents/` and `.claude/skills/`; this wiki explains *why* those agents/skills are built the way they are.
- If a page here conflicts with a specific client project's own `CLAUDE.md` or established conventions, the client project's own rules win for that project — see the [FAQ](../docs/FAQ.md) entry on conflicting conventions.
