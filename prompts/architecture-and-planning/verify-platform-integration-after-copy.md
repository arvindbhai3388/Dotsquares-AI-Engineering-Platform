# Verify a Platform Integration After Copying Files

**Category:** Architecture & Planning
**Use when:** You just copied `CLAUDE.md`, the permissions baseline, and/or specific agents/skills from the Dotsquares AI Engineering Platform into a client project, and need to confirm the copy is actually complete and correct for this project before relying on it.

## Prompt

I just copied Claude Code configuration from the Dotsquares AI Engineering Platform into this project — likely some combination of `CLAUDE.md`, `.claude/settings.json`, and specific agent/skill files. Before I start relying on this setup, verify it end to end:

1. Read the copied `CLAUDE.md` in full and list every remaining `<PLACEHOLDER>` value that still needs to be filled in — quote the exact placeholder text and the section it's in, don't just say "some placeholders remain."
2. Read this project's actual `.csproj`/`.sln` files (or equivalent) to determine its real tech stack, then cross-check every copied agent/skill against that stack. Flag any agent/skill that references a technology this project doesn't actually use (e.g., a Blazor agent in a project with no Blazor project), so I can decide whether to remove it.
3. Based on the same real-stack check, identify anything critical this project's actual stack needs that wasn't copied — a stack-specific agent for a technology genuinely in use here that has no matching file yet.
4. Confirm `.claude/settings.json` (if copied) has had its `_comment` key removed and its allow/ask/deny lists actually match this project's toolchain (e.g., don't leave `dotnet ef` entries in a project with no EF Core/EF6 anywhere).
5. Do **not** modify any application source code as part of this check, and do not guess at project-specific secrets, restricted files, or business context you don't have evidence for — list what you need me to confirm instead of inventing it.
6. Report back as a clear checklist: what's already correct and complete, what's missing, and what I still need to decide or fill in myself.

Stop after producing this report — don't start filling in placeholders or deleting files until I've reviewed it and told you which changes to make.
