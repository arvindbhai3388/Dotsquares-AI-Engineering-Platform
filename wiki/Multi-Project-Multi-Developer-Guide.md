# Multi-Project & Multi-Developer Guide

How this platform behaves across three situations that are easy to conflate but need different habits: **many developers on one project**, **one developer across many projects**, and **many developers across many projects** (the actual day-to-day reality for a 50+ developer, multi-client organization). Read this after [Onboarding Guide](Onboarding-Guide.md) — it assumes you already know the [AI Workflow Discipline](AI-Workflow-Discipline.md) and just need to know how it holds up once more than one project or person is involved.

## The one fact everything else follows from

**This framework is copied into each client project, not centrally installed or referenced live.** Per [docs/Getting-Started.md](../docs/Getting-Started.md), a new client project gets its own copy of `templates/CLAUDE-full.md` (or `-minimal.md`), its own `.claude/settings.json` derived from `templates/permissions-baseline.json`, and typically its own subset of the agents/skills relevant to its stack. Claude Code reads whichever `CLAUDE.md`/`.claude/` config exists in the **current working directory** — there is no global, cross-project state. Every scenario below is a consequence of that one fact.

## Scenario A — Multiple developers, one project

This is the easy case, and mostly already covered by existing docs — summarized here for completeness:

- The project's `CLAUDE.md`, `.claude/agents/`, `.claude/skills/`, and `.claude/settings.json` are committed to that project's own repo, so every developer on the project sees the same rules, the same restricted-file list, and the same permissions baseline — nobody is working from a personally-customized copy.
- Consistency comes from the [Review step](AI-Workflow-Discipline.md) and `/code-review`, not from assuming everyone prompts identically — two developers phrasing a request differently should still converge on the same standard because the same checklist gates both of their diffs.
- If a developer thinks a project's copy of a standard is wrong, the fix is a proposed change to *that project's* `CLAUDE.md`/agent/skill, reviewed like any other change — not a silent personal override (see [FAQ: What if I disagree with a standard](../docs/FAQ.md)).
- New developer joining an existing project: they still go through [Onboarding Guide](Onboarding-Guide.md), but "Days 3–5" narrows immediately to that one project's stack pages, since the project's own `CLAUDE.md` is already decided.

## Scenario B — One developer, multiple projects

This is the scenario most likely to go wrong quietly, because nothing stops you from having two projects' folders open at once, and Claude Code will happily use whichever directory's config is active *for that session* — the risk isn't a technical limitation, it's context bleeding across sessions in your own head (and in an agent's conversation history).

### A real example of what goes wrong

While building the demo/prompt content for this platform itself, a background agent's session carried context from an unrelated client codebase (internal class names like a specific `DbContext` and ADO.NET helper name) into prompts that were supposed to be generic and reusable across *any* client. It was caught in a later audit and fixed, but it's a concrete, real instance of exactly the failure mode this section exists to prevent: **content generated "in the context of" Project A leaking client-specific specifics into Project B (or into shared/generic material) when the same session or the same copy-pasted prompt crosses the boundary.**

### Rules of thumb

1. **One Claude Code session per project, not one session juggling several.** If you're actively switching between Project A and Project B in the same sitting, use separate terminal windows/sessions rather than `cd`-ing back and forth inside one long-running conversation — a long conversation accumulates context, and that context is exactly what leaked in the example above.
2. **Before pasting a prompt from one project into another, generalize it first.** A prompt that worked verbatim on Project A because it named Project A's actual class/table/service names will silently confuse or mislead Claude on Project B. Treat any copy-pasted prompt as a template to re-fill, not a literal instruction.
3. **Verify which project you're in before trusting an agent's proposal.** If a proposal references a file, service, or convention that sounds like it belongs to a *different* project than the one you're currently in, stop and check — that's the leak happening in real time, and it's much cheaper to catch at the Approve step than after Implement.
4. **Don't assume a personal shortcut/snippet is safe to reuse across clients.** Something as small as a helper script, a `.claude/settings.json` permission tweak, or a favorite phrasing you've refined on one client project may embed that client's specifics (a real path, a real service name) without you noticing — review it the same way you'd review any other change before it lands somewhere else.
5. **Each project's restricted-file list is its own.** A file that's safe to read on Project A (because it holds no secrets there) may be exactly the kind of file Project B's `CLAUDE.md` explicitly restricts. Don't let familiarity with one project's restrictions substitute for reading the current project's actual `CLAUDE.md` §2.

### Practical checklist when switching projects

- [ ] Confirm your terminal's working directory actually matches the project you intend to work in (`pwd`/`cd` — don't assume from memory).
- [ ] If picking up a conversation from earlier, skim it for anything project-specific before continuing in a different project's directory — if in doubt, start a fresh session instead of reusing one.
- [ ] Re-read that project's own `CLAUDE.md` if it's been more than a few days since you last touched it — don't rely on a memory of "how this project works" that may have drifted or may be bleeding in from a *different* project you worked on more recently.

## Scenario C — Multiple developers, multiple projects (the org-wide picture)

This is what "50+ developers" actually means in practice: developer-to-project is many-to-many, projects evolve independently after onboarding, and the central platform repo keeps moving too.

### Who owns what

- **This repo** (`Dotsquares-AI-Engineering-Platform`) owns the *shared baseline* — agents, skills, prompts, wiki standards, and the `templates/` that seed a new project. Changes here go through [CONTRIBUTING.md](../CONTRIBUTING.md) and affect every future adopter, not any specific client project directly.
- **Each client project** owns its own copy, and — per [FAQ: How do I pull an update](../docs/FAQ.md) — that copy does **not** automatically update when this repo changes. A developer moving from Project A to Project B should expect Project B's copy of a standard to reflect whatever version of this platform Project B was onboarded (or last updated) against, which may be older or differently-customized than Project A's.
- **No developer should assume their most recent project's conventions are "the framework."** If Project A customized something for a good reason, that customization is Project A's, not a silent update to how the framework itself works — see the same FAQ entry on conflicting conventions.

### Onboarding a new project (quick reference — full detail in [docs/Getting-Started.md](../docs/Getting-Started.md))

1. Copy `templates/CLAUDE-full.md` or `-minimal.md`, fill in every placeholder — do not leave a bracketed placeholder in a committed file.
2. Copy `templates/permissions-baseline.json` to that project's `.claude/settings.json`, adjusted for the project's actual layout.
3. Copy only the agents/skills relevant to that project's actual stack — don't dump all 16 agents into a project that uses 3 of the 11 supported stacks; unused agents are just noise a developer has to read past.
4. Note in that project's own `CHANGELOG.md` (or equivalent) which version/date of the platform it was onboarded against, so a future "did anything change upstream" check (see the FAQ entry) has a starting point to diff from.

### Onboarding a new developer onto an existing project

Follow [Onboarding Guide](Onboarding-Guide.md) in full, but treat "Days 3–5" as reading *that project's* copies of the stack pages/agents, not this repo's originals — they may have already diverged, deliberately, and the project's own copy is the one that actually governs work there.

### If you notice drift between two projects that should probably match

Don't silently "fix" one project to match another, and don't silently update this platform repo based on one project's local customization. Raise it — per [CONTRIBUTING.md](../CONTRIBUTING.md), a change that should apply everywhere is a proposal to this repo; a change that's genuinely specific to one client stays there.

## Related pages

- [Onboarding Guide](Onboarding-Guide.md)
- [AI Workflow Discipline](AI-Workflow-Discipline.md)
- [docs/Getting-Started.md](../docs/Getting-Started.md)
- [docs/FAQ.md](../docs/FAQ.md)
- [CONTRIBUTING.md](../CONTRIBUTING.md)
