# Changelog

All notable changes to the Dotsquares AI Engineering Platform are recorded here. Client projects that adopted this framework by copying `templates/CLAUDE-*.md` (and, since this entry, `templates/permissions-baseline.json`/`templates/mcp-baseline.json`) into their own repo should check this file when they want to know whether anything changed since they last copied — see the FAQ entry "How do I pull an update from this platform into an already-onboarded client project?" in [`docs/FAQ.md`](docs/FAQ.md) for the actual update process.

This file does not follow a strict Semantic Versioning contract (there is no published package to version) — version numbers here mark platform-level milestones, mainly to give client projects a stable reference point to diff against.

## [1.2.0] - Performance/production-safety review, quality-gate aggregation, hooks template

- **New agents:** `performance-reviewer` (N+1 queries, blocking calls, missing pagination, caching gaps — generic across all 11 supported stacks), `production-safety` (database/schema, restricted-config, breaking-API, auth, logging, external-integration, backward-compatibility, and rollback checks — has authority to BLOCK), `quality-gate` (aggregates build-validator/code-reviewer/security-reviewer/performance-reviewer/production-safety into one PASS/WARN/FAIL).
- **New skills:** `performance-review`, `production-safety-check`, `quality-gate` — thin delegating wrappers matching the existing agent pattern.
- **New hooks template:** `templates/hooks/protected-file-guard.ps1` — a `PreToolUse` hook that enforces a project's restricted-files list by script (fails open on any error, only ever blocks/warns, never edits/commits/pushes). See `docs/Hooks-Setup.md`.
- `new-feature` skill's Review step updated to reference all three new agents/skills.
- Fixed a stale claim in `README.md`'s Security section (said "no `SECURITY.md` yet" — it existed since the platform's GitHub publication).
- Agent count: 16 → 19. Skill count: 13 → 16.

## [1.1.0] - QA tracking, streamlined approval, post-copy verification

- **New skill:** `qa-test-tracking` — optional Excel manual-QA workbook, auto-saved with planned test cases at Plan time and auto-updated with real Pass/Fail results at Validate time (never marking a case Pass without an actual test run). Opt in per project via the new `<QA_ARTIFACTS_FOLDER>` placeholder in `templates/CLAUDE-full.md`/`CLAUDE-minimal.md`.
- **New workflow mode:** "Streamlined mode" documented in `wiki/AI-Workflow-Discipline.md` — a developer can give a single Yes/No at Approve that carries through Implement → Test → Review without further per-step check-ins. Never covers commit/push, which remains a separate explicit request every time.
- **New prompt:** `prompts/architecture-and-planning/verify-platform-integration-after-copy.md` — run immediately after copying platform files into a client project; cross-checks the copy against the project's real stack and reports what's missing/unfillable/inapplicable, without modifying anything itself. Referenced from `docs/Getting-Started.md`.
- `new-feature` skill updated to cross-reference both of the above at the relevant steps (Plan/Validate for `qa-test-tracking`, Approve for streamlined mode).
- Prompt count: 217 → 218. Skill count: 12 → 13.

## [1.0.0] - Initial release

Initial release of the platform:

- **Agents** (`.claude/agents/`) — one subagent per supported stack (ASP.NET Core, MVC, Razor Pages, Blazor, Umbraco, EF Core, SQL Server, SignalR, Power BI, SharePoint, Power Apps) plus cross-cutting agents (`code-reviewer`, `architecture-analyst`, `unit-test-writer`, `security-reviewer`, `build-validator`).
- **Skills** (`.claude/skills/`) — reusable slash-command workflows for recurring tasks: new-feature, code-review, unit-testing, architecture-analysis, build-validation, documentation, plus stack-specific workflows (EF Core migrations, Blazor components, SignalR hubs, Power BI embedding, SharePoint/Graph integration, Power Apps connectors).
- **Prompts** (`prompts/`) — 200+ categorized, copy-paste-ready prompts across 12 categories, indexed in `prompts/README.md`.
- **Templates** (`templates/`) — `CLAUDE-full.md`/`CLAUDE-minimal.md` for bootstrapping a new client project's project instructions, `permissions-baseline.json` for `.claude/settings.json`, `mcp-baseline.json` for `.mcp.json`, review/readiness checklists, and 12 per-stack starter-project scaffolds.
- **Wiki** (`wiki/`) — architecture overview, the AI Workflow Discipline (Analyze → Propose → Approve → Implement → Test → Review), per-stack coding standards, integration guides, and the onboarding guide.
- **Docs** (`docs/`) — Getting Started, Claude Code Setup, MCP Setup, Security Guidelines, and FAQ.
- **Demos** (`demos/`) — 3 independently buildable sample projects exercising the framework end-to-end (ASP.NET Core + EF Core API, Blazor + SignalR Dashboard, MVC + Power Platform Integration).
