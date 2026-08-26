# Hooks Setup

What Claude Code hooks are, why this platform ships one template, and how to adapt `templates/hooks/protected-file-guard.ps1` for a new client project.

## What a hook is, here

A Claude Code hook is a script registered in `.claude/settings.json` that runs automatically around a tool call — before it (`PreToolUse`, able to block the call) or after it (`PostToolUse`). It's the one enforcement mechanism in this platform that doesn't depend on Claude choosing to follow an instruction correctly: a restricted-files rule stated in `CLAUDE.md` is a strong convention, but a hook that actually inspects every `Read`/`Edit`/`Write` call and blocks a match is a technical backstop underneath it.

This platform ships exactly one hook template — `templates/hooks/protected-file-guard.ps1` — because it's the one enforcement need genuinely common across every client project: every project on this platform already has its own restricted-files list in its `CLAUDE.md` (§2-equivalent), and this hook is how that list gets enforced by a script instead of by hoping the instruction is remembered every time.

## Why this is a template, not a live hook in this repo

Same reason `templates/CLAUDE-*.md` and `templates/permissions-baseline.json` are templates and not live config here: this platform repo has no restricted-files list of its own worth enforcing, and the actual patterns a hook needs to check are entirely project-specific. Copying the template in and filling in `$RestrictedPatterns` is the adaptation step, the same as filling in `<PLACEHOLDER>`s in `CLAUDE.md`.

## Adapting `templates/hooks/protected-file-guard.ps1` for a new client repo

1. Copy `templates/hooks/protected-file-guard.ps1` to the client project's `.claude/hooks/protected-file-guard.ps1`.
2. Edit `$RestrictedPatterns` at the top of the script to mirror that project's own `CLAUDE.md` restricted-files list **exactly** — the script and the documentation must describe the same set of files, or one of them is lying to whoever reads it.
3. Register it in that project's `.claude/settings.json` under `hooks.PreToolUse`, matching the JSON snippet in the script's own header comment. Use `powershell` (Windows PowerShell 5.1, ships with Windows) unless the target machines are confirmed to have PowerShell 7 (`pwsh`) installed.
4. Test it before relying on it: ask Claude Code to read one of the now-restricted files and confirm the tool call is actually blocked with the hook's message, not silently allowed through.
5. If the project already has other hooks registered, add this one alongside them rather than replacing the existing `PreToolUse` array — a hooks config is additive, not exclusive.

## What this hook does and does not do

- **Fails open on any error.** A malformed input, an unexpected JSON shape, or a bug in the script itself always *allows* the tool call through — this hook is only ever a backstop for the specific patterns it's configured to catch, never a reason legitimate work gets stuck.
- **Only ever blocks or warns.** It never edits, deletes, commits, or pushes anything itself — it inspects a proposed tool call and either lets it through or blocks it with a reason.
- **Does not replace `CLAUDE.md`'s restricted-files documentation** — the two need to stay in sync manually; this hook is additive enforcement, not a substitute for the instruction actually being correct and current.

## Related pages

- [Getting Started](Getting-Started.md)
- [Claude Code Setup](Claude-Code-Setup.md)
- [Security Guidelines](Security-Guidelines.md)
