# Getting Started

This page walks through going from "nothing installed" to a working first Claude Code session against a real client project, using this platform's conventions.

## Prerequisites

- **Claude Code** installed and authenticated — see [Claude Code Setup](Claude-Code-Setup.md) if you haven't done this yet.
- **.NET SDK(s)** matching whichever client project(s) you'll work on. Dotsquares client projects span a wide range of target frameworks (.NET Framework 4.x on older MVC codebases through current .NET LTS releases on newer ones) — install the specific SDK/runtime a project's `.csproj`/`global.json` calls for rather than assuming "latest" is correct.
- **SQL Server** access appropriate to the project — LocalDB or a Developer Edition instance for local work, plus whatever connection details the project's own (gitignored) local configuration expects. Never request or store production connection strings for local development.
- **Git**, and access to the specific client project repository you've been assigned to, granted through your team lead.
- Familiarity with the [Architecture Overview](../wiki/Architecture-Overview.md) and [AI Workflow Discipline](../wiki/AI-Workflow-Discipline.md) wiki pages — read these before your first session if you haven't (see the [Onboarding Guide](../wiki/Onboarding-Guide.md) for the full first-two-weeks path).

## Cloning a client project

Client projects are **separate repositories** from this platform repo — this repo is not vendored into them. A typical setup:

```bash
git clone <client-project-repo-url>
cd <client-project>
```

Client projects reference this platform's standards and conventions but maintain their own `.claude/CLAUDE.md`, their own `.claude/agents`/`.claude/skills` if they've customized any, and their own build/test tooling. Do not assume a client project has this platform repo checked out alongside it or expects you to copy this repo's `.claude/` folder wholesale into it — see the next section for the actual mechanism.

## Dropping in the right `templates/CLAUDE.md`

New client projects (or existing ones being brought onto this framework for the first time) start from a template in this repo's `templates/` folder:

1. Identify which template fits: a **full** template for a client project expected to grow into a multi-stack, long-lived engagement, or a **minimal** template for a small, single-stack, short-lived project where the full template's ceremony isn't worth the overhead.
2. Copy the chosen template to the client project's root as `.claude/CLAUDE.md` (or `CLAUDE.md` at the repo root, matching whatever convention Claude Code discovers automatically for that project's structure — see [Claude Code Setup](Claude-Code-Setup.md) for how project instructions are discovered).
3. **Fill in the project-specific sections** the template leaves as placeholders — restricted files/config patterns specific to that client (mirroring the restricted-files pattern described in [Security Guidelines](Security-Guidelines.md)), the project's actual tech stack and versions, its build/test commands, and any client-specific conventions that should override this platform's defaults.
4. Do not delete the sections referencing this platform's shared standards (coding standards, workflow discipline) unless the client project has an explicit, deliberate reason to diverge — the template exists so every project starts from the same baseline instead of each developer inventing project instructions from scratch.
5. Commit the filled-in `CLAUDE.md` to the client project's own repository — it lives there, not here, once populated for that project.

If you're joining a project that has **already** done this, you don't need to touch `templates/` at all — just read that project's existing `CLAUDE.md`, which already reflects the choices made in step 3 above.

## Dropping in the permissions baseline

Alongside `CLAUDE.md`, a new client project should also start from this repo's permissions template rather than inventing a `.claude/settings.json` from scratch:

1. Copy `templates/permissions-baseline.json` to the client project's root as `.claude/settings.json` (or merge its `allow`/`ask`/`deny` entries into an existing one).
2. Remove the `_comment` key — it's documentation only, not a real Claude Code setting, and left in place it reads as a stray/broken field.
3. Adjust the allow/ask/deny lists for the project's actual layout and toolchain (e.g., a legacy MSBuild-only project doesn't need the `dotnet ef` entries; a project with its own custom secret-shaped config files should add them to the `deny` patterns the same way `appsettings.*.json` is already covered).
4. Commit the filled-in `.claude/settings.json` to the client project's own repository, same as `CLAUDE.md` — see [Claude Code Setup](Claude-Code-Setup.md) for what `settings.json` actually enforces.

If the client project also needs Claude Code wired up to an external system (an issue tracker, a wiki) via MCP, see [MCP Setup](MCP-Setup.md) for the equivalent credential-free starting point (`templates/mcp-baseline.json`).

## Adding a hooks-based technical backstop (optional)

`CLAUDE.md`'s restricted-files section is a strong convention, but it's still an instruction Claude has to follow correctly every time. For a project that wants that enforced by a script instead, see [Hooks Setup](Hooks-Setup.md) for how to adapt `templates/hooks/protected-file-guard.ps1` — it blocks a matching `Read`/`Edit`/`Write` call outright, fails open on any error, and never edits/commits/pushes anything itself.

## Running the post-integration setup prompt

Immediately after copying `CLAUDE.md`, the permissions baseline, and/or specific agents/skills into
the client project (the two sections above), run
[`prompts/architecture-and-planning/verify-platform-integration-after-copy.md`](../prompts/architecture-and-planning/verify-platform-integration-after-copy.md)
as your very first Claude Code session in that project. It checks the copy for you instead of
leaving verification to memory: it lists every `<PLACEHOLDER>` still unfilled, cross-checks the
copied agents/skills against the project's actual `.csproj`/`.sln` stack and flags anything that
doesn't apply, and identifies anything critical the project's real stack needs that wasn't copied.
It's read-only by design — it reports a checklist and stops, it doesn't fill placeholders or delete
files on its own.

## First Claude Code session walkthrough

With the client project cloned and its `CLAUDE.md` in place:

1. Open a terminal in the client project's root directory and launch Claude Code.
2. Claude Code reads the project's `CLAUDE.md` automatically — confirm it picked up the right one by asking something like "what are the restricted files in this project?" and checking the answer matches what you expect from the file you just read.
3. Start with a **read-only, low-stakes** request to get a feel for the workflow discipline in practice — e.g., "explain how [some existing feature] works" (which should invoke an `architecture-analyst`-style agent) rather than immediately asking for a code change.
4. For your first actual change, pick something small (see the [Onboarding Guide](../wiki/Onboarding-Guide.md) day 6–8 guidance) and deliberately go through Analyze → Propose → Approve → Implement → Test → Review rather than accepting the first proposal without reading it.
5. Before considering the change done, run the project's actual build/test command yourself at least once (see that project's `CLAUDE.md` for the correct command — client projects often have mixed toolchains, e.g. MSBuild for a legacy web app alongside `dotnet build`/`dotnet test` for newer SDK-style projects) rather than relying solely on the agent's claim that it ran successfully.

## Where to go next

- [Claude Code Setup](Claude-Code-Setup.md) — permissions model and settings detail.
- [MCP Setup](MCP-Setup.md) — connecting Claude Code to external systems on a client project.
- [Hooks Setup](Hooks-Setup.md) — enforcing the restricted-files list by script, not just instruction.
- [wiki/Home.md](../wiki/Home.md) — full index of standards and integration guides.
- [wiki/AI-Workflow-Discipline.md](../wiki/AI-Workflow-Discipline.md) — the process this whole walkthrough is built around.
- [wiki/Multi-Project-Multi-Developer-Guide.md](../wiki/Multi-Project-Multi-Developer-Guide.md) — read this once you're assigned to more than one project, or a second developer joins yours.
- [FAQ](FAQ.md) — common early questions (ownership of AI-generated code, conflicting client conventions, adding a new stack).
