# Production-Readiness Audit — Dotsquares AI Engineering Platform

**Repository:** `D:\local\Dotsquares-AI-Engineering-Platform`
**Audit type:** Full repository production-readiness review (structure, content, security, build/test verification)
**Audit method:** 5 independent read-only review passes (each scoped to a distinct slice of the repo) plus direct, independent `dotnet build`/`dotnet test` execution against all 3 demo projects. No files were modified as part of this audit.

---

## Executive Summary

The Dotsquares AI Engineering Platform is a substantial, largely well-executed piece of work: 16 agents, 12 skills, 217 prompts, 19 template files, 18 wiki/docs pages, and 3 fully working demo projects (79/79 tests passing, independently re-verified — see below), all built around a consistent stated discipline of analyze → propose → approve → implement → test → review.

The security posture is genuinely clean. An exhaustive grep across the entire tree found no real secrets, no leaked credentials, and no non-placeholder tenant identifiers — every demo correctly uses placeholder GUIDs, `<TENANT_ID>`-style tokens, or credential-free LocalDB connection strings, and the two integration demos (SharePoint/Power BI/Power Apps) make zero outbound network calls, exactly as documented.

However, the repository is **not yet ready for a 50+ developer rollout as-is**, because of four HIGH-severity issues that directly undermine the platform's own core promises:

1. **The repository isn't a git repo and has no `.gitignore`** (root or in 2 of 3 demos), while ~40MB of real build output already sits on disk — the first `git add .` anyone runs will commit binaries and a locally-built `appsettings.Development.json`.
2. **Several wiki/docs/template files reference agent names that don't exist** (`aspnet-core-agent`, `ef-core-agent`, `power-bi-agent`, etc. instead of the real `aspnet-core-developer.md`, `efcore-developer.md`, `powerbi-developer.md`) — a new hire following the Onboarding Guide verbatim would try to invoke agents that aren't there.
3. **The platform's own "one non-negotiable workflow" is stated four different, incompatible ways** across README.md, `wiki/AI-Workflow-Discipline.md`, `docs/Getting-Started.md`, and the `templates/CLAUDE-*.md` files — undermining the single most-repeated concept in the entire repository.
4. **8 of the 217 "generic, reusable" prompts leak one specific client's internal codebase identifiers** (`AutoSearch`, `EmploymentEntities`, `SqlHelper`, `AsWorker`) into what's supposed to be stack-generic content reusable across any client engagement.

None of these are security holes and none of them break a build. All are straightforward, bounded fixes — but they are exactly the kind of inconsistency that erodes trust in a framework whose entire value proposition is *consistency* across 50+ developers. Everything below this level (10 MEDIUM, 15 LOW findings) is genuine polish, not a blocker.

## Overall Score: 79 / 100

Strong foundation, clean security, fully working demos — held back from a higher score by onboarding-breaking reference errors, self-contradictory core messaging, and repo hygiene gaps that are inexpensive to fix.

---

## Independent Build/Test Verification

Per the audit instructions, `dotnet build` and `dotnet test` were re-run independently (not taken on faith from the original build reports) against all 3 demo solutions:

| Demo | `dotnet build` | `dotnet test` |
|---|---|---|
| `Demo1-AspNetCore-EFCore-API` | **Build succeeded** — 0 Warning(s), 0 Error(s) | **Passed** — 21/21, 0 failed, 0 skipped |
| `Demo2-Blazor-SignalR-Dashboard` | **Build succeeded** — 0 Warning(s), 0 Error(s) | **Passed** — 15/15, 0 failed, 0 skipped |
| `Demo3-MVC-PowerPlatform-Integration` | **Build succeeded** — 0 Warning(s), 0 Error(s) | **Passed** — 43/43, 0 failed, 0 skipped |

**Total: 79/79 tests passing across all 3 demos, independently confirmed.** Note: a separate code-quality review counted raw `[Fact]`/`[Theory]` *attribute* occurrences in source (Demo2: 11, Demo3: 34) rather than *executed test cases* — the gap is fully explained by `[Theory]`/`[InlineData]` rows expanding into multiple cases at run time, and is not a discrepancy or defect; the `dotnet test` numbers above are the authoritative, executed-test-case counts.

---

## Architecture Assessment

The repository structure matches what `README.md` and `.claude/CLAUDE.md` claim it contains, verified directory-by-directory: `.claude/{agents,skills}`, `templates/`, `wiki/`, `docs/`, `prompts/`, `demos/` all exist with the exact file counts documented. The layering model described in `wiki/Architecture-Overview.md` (presentation → application/services → data access → external integrations, mapped to which agents own which layer) is a sound and realistic way to organize a multi-stack framework. No architectural anti-patterns were found in how the repo itself is organized.

The one structural weakness is **inconsistency in how demos are laid out**: Demo1 and Demo3 use a `src/<Project>/` + `tests/<Project>.Tests/` split; Demo2 places `DashboardHost` and `SharedComponents` at its own root with no `src/`/`tests/` separation. This isn't wrong, but it means the three demos — meant to teach consistent patterns — don't actually agree on project layout with each other. (See Low Priority Issues.)

## Claude Code Assessment

All 16 agent files and all 12 skill files have valid, well-formed YAML frontmatter with correct required fields, and file layout matches real Claude Code conventions (`.claude/agents/*.md`, `.claude/skills/<name>/SKILL.md`). Nothing found here would cause Claude Code to fail to load an agent or skill — this is a documentation-consistency and completeness gap, not a structural defect.

Two real gaps: no `.mcp.json` or MCP server configuration/template exists anywhere in the repository despite the platform's stated ambition to standardize AI usage across client projects (MCP wiring is legitimately per-client for credentials, but a credential-free template + a docs page is a reasonable expectation for a platform at this scale). Separately, the platform doesn't apply its own `templates/permissions-baseline.json` to itself — there's no `.claude/settings.json` at the repo root, so the template is shipped but never dogfooded or referenced from the onboarding docs.

## Agents Assessment

All 16 files (11 stack-specific + 5 cross-cutting) have concrete, technically specific content — real pitfalls and idioms (DbContext thread-safety, captive-dependency DI bugs, SignalR backplane requirements, Graph least-privilege scoping) rather than generic "write clean code" filler. Tool grants are consistently minimal and correct: read-only reviewers/analysts get `Glob, Grep, Read` only; implementation agents add `Edit, Write, Bash`. The `code-reviewer` / `security-reviewer` split — the overlap pair most likely to collide — is in fact cleanly divided, with `code-reviewer.md` explicitly deferring auth/deserialization/SSRF/crypto findings to `security-reviewer`.

The recurring issue is that **3 of the 5 cross-cutting agents duplicate their paired skill almost line-for-line with no delegation language**, unlike the `code-reviewer`/`code-review` pair, which does this correctly (see Medium Priority Issues #1).

## Skills Assessment

All 12 skills have actionable, step-by-step workflows rather than vague guidance, and correctly enforce the platform's Test-First/analyze-propose-approve discipline. The same duplication issue noted above applies symmetrically here (`unit-testing`, `architecture-analysis`, `build-validation` skills vs. their paired agents). Additionally, bUnit testing guidance is written out in near-full duplicate across three separate files (`unit-test-writer.md`, `blazor-component/SKILL.md`, `blazor-developer.md`) — a maintenance risk if that guidance ever needs to change.

## Prompt Library Assessment

Verified against the strictest, most mechanical checks available: **the `prompts/README.md` index is 100% accurate** — all 217 files are indexed, no dead links, per-category counts match exactly (24/16/21/16/22/21/16/16/16/16/21/12 = 217). All 44 sampled files (spread across all 12 categories) follow the required template with zero deviation. Ten-plus spot-checked technical claims (EF Core `ExecuteUpdateAsync`, Blazor `EditContext.OnFieldChanged`, Dataverse `IPlugin`, SQL Server DMVs, etc.) all correspond to real, current APIs — no fabricated or deprecated references found. Apparent near-duplicate prompt pairs (e.g., two SignalR reconnection prompts, two Power BI refresh prompts) were investigated individually and found to be deliberately scoped, cross-referenced companion prompts, not redundant content.

The one real defect — and the most consequential prompt-library finding — is that **8 files across 4 categories leak one specific client codebase's internal identifiers** (`AutoSearch`, `EmploymentEntities`, `SqlHelper`, `AsWorker`) into prompts that are supposed to be generic and reusable across any Dotsquares client engagement. This is a straightforward find-and-generalize fix, not a structural problem, but it directly contradicts the library's own stated design goal.

## Templates Assessment

All 19 files exist as specified, `permissions-baseline.json` is valid JSON with no embedded secrets, and both `CLAUDE-full.md`/`CLAUDE-minimal.md` use clearly bracketed placeholders with no ambiguous leftover text — `CLAUDE-full.md` §4 enumerates the same 11 stacks, in the same order, as `README.md`. The 12 `starter-projects/<stack>/` folders (vs. 11 README stack bullets) is explained by Blazor correctly getting two scaffolds (Server + WASM) — a defensible choice that is simply never stated explicitly anywhere (Low).

The template checklists (`code-review-checklist.md`, `pre-implementation-checklist.md`, `production-readiness-checklist.md`) are well-written but **orphaned** — no agent or skill that logically should use them (`code-reviewer.md`, `new-feature/SKILL.md`) actually references them, creating two independently-maintained copies of "the review checklist" that can silently drift.

## Documentation Assessment

The headline, most mechanically verifiable check — broken cross-references — came back **completely clean**: zero broken relative Markdown links across all 37 scanned files in `wiki/`, `docs/`, `README.md`, and `templates/`, including a case-sensitivity check for eventual Linux CI. `wiki/Home.md` correctly indexes all 13 other wiki pages and all 4 docs pages.

Two real gaps found: (1) the 3 demo projects' own READMEs are never linked from README.md/docs/wiki — a developer following the documented onboarding path is handed a folder name, never a direct link to a runnable example; (2) `docs/Security-Guidelines.md` line 55 contains a malformed Markdown reference-link definition that will render incorrectly. Separately (and more significantly, covered under High Priority), several docs/wiki pages reference agent names that don't exist on disk.

## Demo Projects Assessment

All three demos are, in the words of the independent code-quality review, "honest and well-built" as **teaching examples**: no hardcoded secrets or real tenant credentials anywhere, all data access is parameterized/LINQ-based, Demo3's "mock now / real later" seam makes genuinely zero outbound network calls (verified by grep — every `http(s)://` hit is either a doc-comment describing a *real* future implementation or a hardcoded fake display string), and every README's stated run/test instructions match the actual code and `global.json` SDK pin.

The one real gap: **Demo2's `DashboardHost` project (the SignalR hub, the background broadcaster, and the settings singleton) has zero automated test coverage** — only the `SharedComponents` Razor Class Library is tested. Demo1's SignalR test coverage also stops one step short of proving the actual broadcast fires (it only confirms the hub route is mapped). Everything else found in the demos (a pinned older xUnit version, one package-version drift, Swagger not gated behind `IsDevelopment()` in Demo3, a missing upload size/type guardrail, no static-analysis configuration) is minor and does not affect the demos' validity as teaching examples.

## Security Assessment

**Clean.** An exhaustive grep across the entire repository (excluding `bin/`/`obj/`) for connection-string credentials, API-key shapes, JWT/AWS/PEM patterns, and non-placeholder GUIDs found nothing but correctly-used placeholders. Specifically verified:
- All GUIDs are either standard `.sln` MSBuild boilerplate constants or the explicit `00000000-0000-0000-0000-000000000000` placeholder used consistently.
- `Demo1`'s connection string uses LocalDB with Windows integrated auth (`Trusted_Connection=True`) — no credential present.
- `Demo3`'s `appsettings.json` uses `<TENANT_ID>`/`<SHAREPOINT_SITE_ID>`-style tokens with explicit comments confirming the mock services never call real APIs.
- No `HttpClient` is even registered in Demo3's `Program.cs` — the "mock now, real later" seam has no way to accidentally reach a real endpoint.
- Mentions of `ClientSecret`/`ApiKey`/`ConnectionStrings` in docs/wiki/prompts are exclusively concept/property names in guidance text, never real values.

The only security-adjacent gap is process, not a leak: 2 of 3 demos have no `.gitignore`, so `appsettings.Development.json` (currently containing only logging config, no secrets) isn't actually protected from being committed the way `.claude/CLAUDE.md`'s own security principle claims it is.

## Maintainability Assessment

The largest maintainability risk is the workflow-phrasing drift and duplicate agent/skill/checklist content described above — every one of these is a place where the platform can silently diverge from itself as it's edited over time, with no cross-references in place to catch the drift. The absence of a `CHANGELOG.md` or any versioning compounds this at the framework level: client projects adopt this platform by *copying* `templates/CLAUDE-*.md` into their own repo (not by referencing it live), so there is currently no way for an already-onboarded client project to know the shared framework has changed, or how to pull in an update.

## Developer Experience Assessment

For a developer following the documented path (README → `docs/Getting-Started.md` → `wiki/Home.md`), the link graph itself is fully navigable with zero dead ends — a genuine strength. The failure mode is one level deeper: once that developer tries to *act* on what they read (invoke the agent named in the Onboarding Guide, find a runnable demo, use the review checklist), several of those instructions point at things that don't exist or aren't connected up (non-existent agent names, unlinked demo READMEs, orphaned checklists). The reading experience is good; the "instructions actually work end-to-end" experience has real gaps.

---

## Critical Issues

**None found.** No security leaks, no build failures, no data-loss risks, and nothing that would prevent the repository from being used today in its current state.

---

## High Priority Issues

### H1 — No git repository, no `.gitignore` anywhere at root, and 2 of 3 demos have no `.gitignore` at all, while real build output already exists on disk
- **File path:** repository root; `demos/Demo2-Blazor-SignalR-Dashboard/`; `demos/Demo3-MVC-PowerPlatform-Integration/`
- **Why it's an issue:** `D:\local\Dotsquares-AI-Engineering-Platform` is not currently a git repository and has no root `.gitignore`. Only `demos/Demo1-AspNetCore-EFCore-API/.gitignore` exists. Meanwhile, ~40MB of real `bin`/`obj` build output already sits on disk across all 3 demos, including a locally-built copy of `appsettings.Development.json`. The very first `git init && git add .` anyone runs on this repo will commit binaries and a local config file.
- **Recommended fix:** Add a root `.gitignore` (covering `bin/`, `obj/`, `*.user`, `.vs/`, `appsettings.Development.json`, `appsettings.Local.json`, `*.suo`) before this repo is ever initialized as a git repo. Add matching per-demo `.gitignore` files to Demo2 and Demo3, mirroring Demo1's.

### H2 — Wiki/docs/template files reference agent names that don't exist
- **File path:** `wiki/Architecture-Overview.md` (lines ~60-63), `wiki/Onboarding-Guide.md` (line ~57), `docs/FAQ.md` (line ~24), `templates/code-review-checklist.md` (line ~5)
- **Why it's an issue:** These files reference agent identifiers like `aspnet-core-agent`, `aspnet-mvc-agent`, `ef-core-agent`, `power-bi-agent`, `power-apps-agent`, `sql-server-agent` — none of which exist. The real files in `.claude/agents/` are `aspnet-core-developer.md`, `mvc-developer.md`, `efcore-developer.md`, `powerbi-developer.md`, `powerapps-developer.md`, `sql-server-developer.md`, etc. (`-developer` suffix, different hyphenation for `efcore`/`powerbi`/`powerapps`). A new hire following `Onboarding-Guide.md` — the exact document meant to prevent this kind of confusion — would try to invoke an agent that doesn't exist.
- **Recommended fix:** Update all four files to use the real agent filenames/identifiers exactly as they exist in `.claude/agents/`.

### H3 — The platform's core workflow is stated four different, incompatible ways across its own canonical documents
- **File path:** `README.md` (line ~46: `Understand → Locate → Plan → Approve → Implement → Test → Review`), `wiki/AI-Workflow-Discipline.md` (line ~6: `Analyze → Propose → Approve → Implement → Test → Review`), `docs/Getting-Started.md` (line ~43, matches the wiki's phrasing), `templates/CLAUDE-minimal.md` (line ~32: a 7-step `Test-First`/`Validate` variant), `templates/CLAUDE-full.md` (lines ~279-287: an 8-step `Understand → Locate → Inspect → Plan → Test-First → Implement → Validate → Review` variant, also used in `templates/pre-implementation-checklist.md`)
- **Why it's an issue:** README.md calls this "one non-negotiable workflow" that "every agent, skill, and prompt in this repository is built around," and directs readers to `wiki/AI-Workflow-Discipline.md` for "the full rationale" — but that exact page uses different step names (`Analyze`/`Propose` vs. `Understand`/`Plan`/`Approve`) than the README that cites it. Two more variants exist in the CLAUDE.md templates. This is the single most-referenced concept in the repository, and it doesn't agree with itself.
- **Recommended fix:** Standardize on one canonical phrasing/step-count for the platform-level discipline (used consistently in README/wiki/agents/skills/prompts), and if the more granular 7-8 step CLAUDE.md-template version is intentionally different (more concrete for day-to-day ticket work), state that relationship explicitly rather than leaving four unreconciled versions.

### H4 — 8 of 217 prompts leak one specific client's internal codebase identifiers into "generic, reusable" content
- **File path:** `prompts/sql-server/convert-dynamic-sql-to-parameterized.md`, `prompts/sql-server/optimize-bulk-insert.md`, `prompts/code-review-and-testing/review-for-thread-safety-issues.md`, `prompts/code-review-and-testing/review-diff-for-performance-regressions.md`, `prompts/code-review-and-testing/add-mstest-tests-for-legacy-method.md`, `prompts/mvc-razor/add-razor-view-component.md`, `prompts/blazor/add-dependency-injection-scoped-service-blazor.md`, `prompts/sharepoint/sync-sharepoint-list-to-sql-database.md`
- **Why it's an issue:** The prompt library is explicitly designed to be generic and reusable across any client engagement. These 8 files instead name a specific client's internal ASP.NET/EF6 solution and its actual class names (`AutoSearch`, `EmploymentEntities`, `SqlHelper`, `AsWorker`) rather than describing the concept generically (e.g., "your DbContext," "this project's existing ADO.NET helper"), which every other prompt in the library correctly does.
- **Recommended fix:** Replace the hardcoded names with generic placeholders consistent with the rest of the library. This is a same-day text edit across 8 files with no structural changes needed.

---

## Medium Priority Issues

### M1 — Three cross-cutting agent/skill pairs duplicate content with no delegation language
- **File path:** `.claude/agents/unit-test-writer.md` + `.claude/skills/unit-testing/SKILL.md`; `.claude/agents/architecture-analyst.md` + `.claude/skills/architecture-analysis/SKILL.md`; `.claude/agents/build-validator.md` + `.claude/skills/build-validation/SKILL.md`
- **Why it's an issue:** Each pair has near-identical trigger phrases and near-duplicate workflow content, with neither file mentioning the other — unlike `code-reviewer.md`/`code-review/SKILL.md`, which correctly and explicitly delegates.
- **Recommended fix:** Add one clarifying sentence to each file's description establishing the split (skill = enforced workflow entry point; agent = delegate that performs the work), mirroring the pattern already used elsewhere in the repo (e.g., `blazor-developer`/`blazor-component`).

### M2 — No MCP configuration or template exists anywhere in the repository
- **File path:** repository-wide (no `.mcp.json` found)
- **Why it's an issue:** For a platform whose stated goal is to standardize AI usage across client projects, the total absence of even a credential-free MCP template (for common integrations like Jira/Confluence/Azure DevOps) is a real, if not severe, gap relative to that ambition.
- **Recommended fix:** Add a `templates/mcp-baseline.json` (placeholder server entries) and a short `docs/MCP-Setup.md`.

### M3 — `permissions-baseline.json` exists but is never referenced from onboarding docs
- **File path:** `templates/permissions-baseline.json`; `docs/Getting-Started.md`; `docs/Claude-Code-Setup.md`
- **Why it's an issue:** `docs/Claude-Code-Setup.md` reinvents a trivial 3-line permissions example instead of pointing to this existing, much more complete template that lives in the same `templates/` folder `Getting-Started.md` already walks developers through for `CLAUDE.md`.
- **Recommended fix:** Add a step to the onboarding flow telling new client projects to copy `templates/permissions-baseline.json` to `.claude/settings.json`, the same way `CLAUDE-*.md` is already handled.

### M4 — The framework repo doesn't dogfood its own permissions template
- **File path:** `.claude/` (no `settings.json` at repo root)
- **Why it's an issue:** The repo ships a permissions baseline for clients but doesn't apply it to itself, missing a chance to validate the template and serve as a live example.
- **Recommended fix:** Add a `.claude/settings.json` at the repo root derived from `templates/permissions-baseline.json`.

### M5 — Malformed Markdown reference-link syntax
- **File path:** `docs/Security-Guidelines.md` (line 55)
- **Why it's an issue:** The line is written as a reference-link definition (`[label]: ...`) followed by inline `[text](url)` links on the same line — invalid/undefined Markdown syntax that will render incorrectly depending on the renderer. Every other "related pages" bullet in the repo uses plain `- [Text](path)`.
- **Recommended fix:** Rewrite as ordinary list bullets matching the rest of the file's convention.

### M6 — Demo project READMEs are orphaned from the documented onboarding path
- **File path:** `README.md`; `docs/Getting-Started.md`; `docs/FAQ.md`
- **Why it's an issue:** None of the 3 demo projects' own READMEs are ever linked from README.md, wiki/, or docs/ — only mentioned as bare folder names. A developer following the documented onboarding path is never handed a direct link to a runnable example.
- **Recommended fix:** Add a short "Demo projects" table to `wiki/Home.md` or `docs/Getting-Started.md` linking each demo's README directly.

### M7 — Template checklists exist but are unreferenced by the agents/skills that should use them
- **File path:** `templates/code-review-checklist.md`, `templates/pre-implementation-checklist.md`, `templates/production-readiness-checklist.md`; `.claude/agents/code-reviewer.md`; `.claude/skills/new-feature/SKILL.md`
- **Why it's an issue:** Each checklist states its own purpose (e.g., "basis for a `code-reviewer`-agent pass"), but the relevant agent/skill carries its own independently-written inline checklist instead, creating two copies that can silently drift out of sync.
- **Recommended fix:** Either have the agents/skills explicitly reference these template files as canonical, or state explicitly that the duplication is intentional (client-repo-facing artifact vs. platform's internal checklist).

### M8 — Secrets-handling guidance duplicated without cross-reference
- **File path:** `.claude/CLAUDE.md` (line ~11); `docs/Security-Guidelines.md` (lines 5-22)
- **Why it's an issue:** Both files independently state the same secrets-handling rule; `docs/Security-Guidelines.md` is considerably more detailed, but neither file references the other, so a future edit to one can drift from the other unnoticed.
- **Recommended fix:** Make `.claude/CLAUDE.md`'s secrets bullet a one-line summary that links to `docs/Security-Guidelines.md` as the single source of truth.

### M9 — No versioning/changelog, and no documented update path for already-onboarded client projects
- **File path:** repository-wide; `docs/Getting-Started.md`; `docs/FAQ.md`
- **Why it's an issue:** Client projects adopt this framework by *copying* `templates/CLAUDE-*.md` into their own repo, not by referencing it live. There is no `CHANGELOG.md`, no version marker, and no documented mechanism for an already-onboarded project to learn the shared framework changed or pull in an update.
- **Recommended fix:** Add a `CHANGELOG.md` at the platform root and a short "how to pull an update" section to `docs/FAQ.md` or `docs/Getting-Started.md`.

### M10 — Demo2's `DashboardHost` has zero automated test coverage
- **File path:** `demos/Demo2-Blazor-SignalR-Dashboard/DashboardHost/` (no corresponding `DashboardHost.Tests` project)
- **Why it's an issue:** `MetricsHub`, `MetricsBroadcastService` (which swallows exceptions), and `DashboardSettingsService` (a locked shared-state singleton) have no automated coverage at all — only the `SharedComponents` Razor Class Library is tested, undercutting the test-coverage bar the other two demos clear comfortably.
- **Recommended fix:** Add a `DashboardHost.Tests` xUnit project mirroring Demo1/Demo3's `src`/`tests` pattern, covering `DashboardSettingsService`'s update/read behavior and `MetricsBroadcastService`'s threshold logic, plus a hub negotiate/connection test similar to Demo1's.

---

## Low Priority Issues

### L1 — bUnit guidance duplicated across three files
`.claude/agents/unit-test-writer.md`, `.claude/skills/blazor-component/SKILL.md`, `.claude/agents/blazor-developer.md` all restate near-identical bUnit idiom guidance. **Fix:** keep the canonical version in `unit-test-writer.md` and have the others reference it.

### L2 — "Razor" should read "Razor Pages" in the repo map
`.claude/CLAUDE.md` line ~21 lists "Razor" in its agent-stack list, while the actual file is `razor-pages-developer.md` and the README correctly says "Razor Pages." **Fix:** align the wording.

### L3 — Inconsistent hyphenation in one wiki filename
`wiki/SQL-Server-Guidelines.md` inserts a hyphen inside the product name, unlike `EFCore-Guidelines.md`, `PowerBI-Integration.md`, `PowerApps-Integration.md`. **Fix:** cosmetic only, not worth a rename/link-update churn on its own — document the convention for future additions.

### L4 — Starter-project folder count (12) vs. README stack count (11) not explained
`templates/starter-projects/` has 12 folders because Blazor correctly gets two scaffolds (Server + WASM), but nothing states this mapping. **Fix:** one clarifying line in README's repository-layout section.

### L5 — No `CONTRIBUTING.md`
No formal document describes how any of the 50+ developers propose a change to a shared agent/skill/wiki page, or who approves platform-level changes. **Fix:** add a root `CONTRIBUTING.md`.

### L6 — No root `LICENSE`
No statement of Dotsquares's internal usage terms for the framework content itself (third-party licenses for vendored front-end libraries in Demo3 do exist). **Fix:** add an internal-use `LICENSE`/`NOTICE`.

### L7 — Vendored front-end libraries committed directly in Demo3, no LibMan manifest
`demos/Demo3-MVC-PowerPlatform-Integration/.../wwwroot/lib/{bootstrap,jquery,jquery-validation*}/` are committed as full distributions with no `libman.json`, working against `.claude/CLAUDE.md` §5's stated preference for built-in capabilities. **Fix:** add a `libman.json`, or note the manual-vendoring choice in the demo's README.

### L8 — Demo1's xUnit version is pinned to an older release with no rationale noted
`tests/TaskTracker.Tests/TaskTracker.Tests.csproj` pins `xunit`/`xunit.runner.visualstudio` to `2.5.3`. **Fix:** bump to current 2.9.x, or document the reason if intentional.

### L9 — Demo1's SignalR test coverage stops at "route is mapped," not "broadcast actually fires"
No test opens a real `HubConnection` and asserts a `TaskStatusChanged` message is received. **Fix:** add one `HubConnection`-based integration test against the `WebApplicationFactory` test server.

### L10 — Demo2 has an unexplained package-version drift within the same solution
`Microsoft.AspNetCore.SignalR.Client` pinned to `8.0.11` vs. `Microsoft.AspNetCore.Components.Web` at `8.0.25`. **Fix:** align both to the same 8.0.x patch level or document why they differ.

### L11 — Demo3's Swagger UI is not gated behind `IsDevelopment()`
Unlike Demo1, which correctly wraps Swagger mapping in an `IsDevelopment()` check, Demo3 maps it unconditionally. **Fix:** apply the same gate for consistency and to model the safer default.

### L12 — Demo3's file upload endpoint has no size/type guardrail
`DocumentsController.Upload` only checks for a null/empty file. Harmless today (the mock never persists bytes), but risky if copied as a starting point for a real implementation. **Fix:** add a `[RequestSizeLimit]`/allowlist and a comment noting the gap.

### L13 — Cross-demo folder layout is inconsistent
Demo1/Demo3 use `src/`+`tests/`; Demo2 does not. **Fix:** document the Demo2 deviation (Razor Class Library pattern) explicitly, or align the layout.

### L14 — No static analysis / `TreatWarningsAsErrors` configured in any demo
Consistent across all three (not an inconsistency between them), but notable for "enterprise .NET delivery" teaching examples. **Fix:** consider enabling `EnableNETAnalyzers`, or note the omission is intentional to stay minimal.

### L15 — Apparent prompt-library duplicates, investigated and resolved as non-issues
Several superficially similar prompt pairs (SignalR reconnection, Power BI refresh, multi-tenant EF Core vs. architecture-level multi-tenant strategy) were checked individually and found to be deliberately scoped, cross-referenced companions — noted here only for completeness, no action needed.

---

## Recommended Improvements

Beyond fixing the issues above, in priority order for the next iteration:

1. **Fix the 4 High-severity items first** — they're all bounded, mechanical fixes (add `.gitignore`s, correct agent names in 4 files, pick one workflow phrasing, generalize 8 prompt files) and they're the ones most likely to visibly embarrass the platform in front of a new hire or a client.
2. **Establish a single source of truth for the core workflow** and have every other file link to it rather than restate it — this same pattern (one canonical doc + links, not restatement) should also resolve M7 and M8.
3. **Add a lightweight platform versioning scheme** (even just a `CHANGELOG.md` with dated entries) before this is rolled out to 50+ developers across many client repos that will inevitably ask "did anything change since I copied my CLAUDE.md?"
4. **Close Demo2's test gap** before holding it up as the reference example for SignalR-hub testing patterns, since right now Demo1 (despite its own minor gap) is the stronger example of that pattern.
5. **Initialize git with a proper `.gitignore` in place from commit one** — do this before any further work happens in the repo, since every day that passes increases the amount of build output that would need to be cleaned out of history later.

---

## Final Go/No-Go Recommendation

**CONDITIONAL GO.**

This platform should not be rolled out to 50+ developers in its current state, but it is close — a focused remediation pass on the 4 High-severity findings (repo/git hygiene, broken agent-name references, workflow-phrasing consistency, prompt-library client-data leakage) is realistically a 1-2 day effort, not a redesign. Nothing found in this audit — including the security sweep, which came back completely clean — indicates a fundamental flaw in the platform's design or approach. The underlying content (agents, skills, prompts, wiki, demos) is consistently well-executed and technically accurate; the gaps are almost entirely in the connective tissue between pieces (cross-references, naming agreement, workflow phrasing) rather than in the pieces themselves.

**Recommendation:** Fix all 4 High findings, then re-audit those specific items (not a full re-audit) before rollout. Medium and Low findings can be tracked and addressed as fast-follow work without blocking initial adoption.
