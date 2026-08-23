# Claude Code Setup

How to install Claude Code, configure the basics of `.claude/settings.json`, understand its permissions model, and how this repository's agents/skills actually get discovered and used.

## Installing Claude Code

Claude Code is installed as a standalone CLI (via the platform-appropriate installer/npm package documented by Anthropic) and authenticated against your organization's Claude account/API access. Dotsquares developers should authenticate with their work identity so usage is attributable and billed correctly at the organization level — do not authenticate a work installation against a personal account.

After installation, verify it launches correctly from within a project directory (`claude` from the repo root, or your editor's Claude Code integration if you're using one) before relying on it for real work.

## `.claude/settings.json` basics

Claude Code project configuration lives under a `.claude/` directory at a project's root (this platform repo has one; each client project maintains its own):

- **`.claude/settings.json`** — checked into source control, shared by the whole team working on that project. Holds team-wide defaults: which tools are allowed/denied by default, hook configuration, and other settings meant to be consistent for everyone on the project.
- **`.claude/settings.local.json`** — personal overrides for one developer's machine, not checked in (should be gitignored). Use this for anything genuinely personal to your setup rather than editing the shared file for a personal preference.
- **`CLAUDE.md`** (at the repo root, or `.claude/CLAUDE.md`) — not settings in the permissions sense, but project *instructions* Claude Code reads automatically at the start of a session. This is where the rules described throughout this wiki/docs set actually get enforced per project — see [Getting Started](Getting-Started.md) for how a client project's `CLAUDE.md` gets populated from this repo's `templates/`.

Don't write a `settings.json` from scratch for a new client project — start from `templates/permissions-baseline.json` in this repo (see [Getting Started](Getting-Started.md) for the copy-in steps). It already encodes a reasonable .NET-project default rather than the trivial three-line sketch a team might otherwise improvise, for example:

```json
{
  "permissions": {
    "allow": ["Bash(git status)", "Bash(dotnet build:*)", "Bash(dotnet test:*)", "Read(**)", "Grep(**)", "Glob(**)"],
    "ask": ["Bash(git commit:*)", "Bash(dotnet publish:*)", "Edit(**/appsettings.Development.json)"],
    "deny": ["Bash(git push --force:*)", "Bash(rm -rf:*)", "Read(**/appsettings.json)", "Read(**/*.pfx)"],
    "defaultMode": "ask"
  }
}
```

(abridged — the real template has the full allow/ask/deny lists, including the restricted-file `deny` patterns matching this repo's own [Security Guidelines](Security-Guidelines.md)). Treat the copied-in template as a starting point, not a fixed prescription — the right allow/ask/deny split depends on how much a given project's team trusts autonomous edits versus wanting a prompt on every write, and can reasonably differ between a low-risk demo project and a production client codebase. If the project also needs an MCP server connection, see [MCP Setup](MCP-Setup.md) for the equivalent `templates/mcp-baseline.json` starting point.

## Permissions model

Claude Code's permission system governs which tool calls require explicit human approval before executing, independent of anything a `CLAUDE.md` says. This is a genuine safety boundary, not documentation — it is enforced by the harness itself:

- **Allow** — the tool/command runs without prompting. Reserve this for read-only or clearly low-risk operations (reading files, searching, running an already-trusted build command) once you've built confidence in the workflow.
- **Ask** — the default for anything that changes state: file writes/edits, running arbitrary shell commands, git operations. You are prompted to approve before it executes.
- **Deny** — the tool/command is blocked outright, regardless of what's asked for. Use this for genuinely dangerous operations a project never wants automated (e.g., destructive git operations, deleting files outside the working tree).
- Permission granularity extends to specific command patterns (e.g., allow `git status`/`git diff` but ask for `git commit`, allow `dotnet build` but ask for `dotnet publish`) — tune this per project rather than an all-or-nothing stance on an entire tool category.
- A `CLAUDE.md` instruction (e.g., "never auto-commit") is a **behavioral instruction** Claude is expected to follow; the `settings.json` permission system is the **enforced backstop** for the subset of actions it covers. Rely on both together, not `CLAUDE.md` alone, for anything genuinely high-stakes (secrets, destructive operations, production access) — see [Security Guidelines](Security-Guidelines.md).

## How agents and skills get discovered

- **Agents** (`.claude/agents/*.md`) are specialized subagent definitions — each file's frontmatter and body describe what the agent is for, what tools it can use, and how it should behave. Claude Code surfaces them as invocable subagents; a stack-specific agent (e.g., a Blazor agent) is written to bring that stack's specific constraints (see [wiki/Coding-Standards-Blazor.md](../wiki/Coding-Standards-Blazor.md)) to bear rather than giving generic .NET advice.
- **Skills** (`.claude/skills/<name>/SKILL.md`) are reusable, named workflows — invoked as slash commands (e.g., `/code-review`, `/unit-testing`) — that encode a specific multi-step process (see [wiki/AI-Workflow-Discipline.md](../wiki/AI-Workflow-Discipline.md) for why these exist as enforced workflows rather than informal habits).
- Both are discovered automatically from the project's `.claude/` directory when Claude Code starts a session in that project — no separate registration step is required beyond the files existing at the expected paths.
- A client project can define its **own** agents/skills under its own `.claude/` directory, which take precedence for that project over anything generic — this is the intended mechanism for a client project to encode conventions specific to itself (a particular internal library, an unusual deployment process) without needing to fork this platform repo.
- This platform repository's own `.claude/agents` and `.claude/skills` are the shared, cross-client baseline — see the [Architecture Overview](../wiki/Architecture-Overview.md) for how each stack agent maps onto a layer of a typical solution.

## Verifying your setup

After installing and cloning a client project, confirm:

1. `claude` launches from the project root without error.
2. Asking "what does this project's CLAUDE.md say about restricted files?" returns an answer matching what you see when you read that file yourself.
3. A trivial read-only request (e.g., "list the top-level folders in this project") completes without unexpected permission prompts, and a file-editing request does prompt for approval (unless your project has deliberately configured otherwise).

## Related pages

- [Getting Started](Getting-Started.md)
- [MCP Setup](MCP-Setup.md)
- [Security Guidelines](Security-Guidelines.md)
- [wiki/AI-Workflow-Discipline.md](../wiki/AI-Workflow-Discipline.md)
