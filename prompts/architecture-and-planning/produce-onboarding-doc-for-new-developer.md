# Produce an Onboarding Document for a New Developer

**Category:** Architecture & Planning
**Use when:** A new team member needs to ramp up on an unfamiliar, under-documented part of the codebase quickly.

## Prompt

Produce an onboarding document for a new developer joining the legacy module described below, written for someone who has no prior context on this codebase. This is a documentation task only — do not modify any source files.

Read through the module's actual code (entry points, core classes, configuration, and its immediate dependencies/callers) rather than relying solely on any existing documentation, since legacy modules often drift from their docs. If existing documentation does exist, note where it agrees or disagrees with what the code actually does.

Structure the document as:

1. **What this module does** — a plain-language summary of its purpose and where it sits in the overall system (what calls it, what it calls).
2. **Architecture overview** — the module's internal structure (key classes/services and their responsibilities), its data storage (tables, files, external stores), and any non-obvious design decisions baked into the code (e.g., reflection-based plugin loading, a homegrown queue, a legacy framework version) that a newcomer needs to know before making changes.
3. **Key files to read first** — a short, ordered list (5-10 files) of the files most worth reading to understand the module, with a one-line reason for each.
4. **How to run/build/test it locally** — the actual commands and toolchain required (noting any non-standard build requirements, e.g., a non-SDK-style project needing MSBuild instead of the dotnet CLI, or a specific test framework/project pairing).
5. **Gotchas** — concrete traps a newcomer would likely hit: implicit assumptions, global/shared state, naming that's misleading relative to actual behavior, configuration that must be set a specific way, or fragile code paths that break easily on changes.
6. **Common tasks and where to start** — for 2-3 realistic types of changes someone might be asked to make in this module, name the starting file/class and the general approach, without writing the change itself.
7. **Who/what to check before changing X** — any cross-team or cross-service dependency a change here could silently affect.
8. **Open questions/unknowns** — anything you could not determine from the code alone and would need to ask an existing team member about.

Write this as a standalone Markdown document suitable for a project's `Notes/` or `docs/` folder.
