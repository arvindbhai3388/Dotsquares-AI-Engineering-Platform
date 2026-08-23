# Changelog

All notable changes to the Dotsquares AI Engineering Platform are recorded here. Client projects that adopted this framework by copying `templates/CLAUDE-*.md` (and, since this entry, `templates/permissions-baseline.json`/`templates/mcp-baseline.json`) into their own repo should check this file when they want to know whether anything changed since they last copied — see the FAQ entry "How do I pull an update from this platform into an already-onboarded client project?" in [`docs/FAQ.md`](docs/FAQ.md) for the actual update process.

This file does not follow a strict Semantic Versioning contract (there is no published package to version) — version numbers here mark platform-level milestones, mainly to give client projects a stable reference point to diff against.

## [1.0.0] - Initial release

Initial release of the platform:

- **Agents** (`.claude/agents/`) — one subagent per supported stack (ASP.NET Core, MVC, Razor Pages, Blazor, Umbraco, EF Core, SQL Server, SignalR, Power BI, SharePoint, Power Apps) plus cross-cutting agents (`code-reviewer`, `architecture-analyst`, `unit-test-writer`, `security-reviewer`, `build-validator`).
- **Skills** (`.claude/skills/`) — reusable slash-command workflows for recurring tasks: new-feature, code-review, unit-testing, architecture-analysis, build-validation, documentation, plus stack-specific workflows (EF Core migrations, Blazor components, SignalR hubs, Power BI embedding, SharePoint/Graph integration, Power Apps connectors).
- **Prompts** (`prompts/`) — 200+ categorized, copy-paste-ready prompts across 12 categories, indexed in `prompts/README.md`.
- **Templates** (`templates/`) — `CLAUDE-full.md`/`CLAUDE-minimal.md` for bootstrapping a new client project's project instructions, `permissions-baseline.json` for `.claude/settings.json`, `mcp-baseline.json` for `.mcp.json`, review/readiness checklists, and 12 per-stack starter-project scaffolds.
- **Wiki** (`wiki/`) — architecture overview, the AI Workflow Discipline (Analyze → Propose → Approve → Implement → Test → Review), per-stack coding standards, integration guides, and the onboarding guide.
- **Docs** (`docs/`) — Getting Started, Claude Code Setup, MCP Setup, Security Guidelines, and FAQ.
- **Demos** (`demos/`) — 3 independently buildable sample projects exercising the framework end-to-end (ASP.NET Core + EF Core API, Blazor + SignalR Dashboard, MVC + Power Platform Integration).
