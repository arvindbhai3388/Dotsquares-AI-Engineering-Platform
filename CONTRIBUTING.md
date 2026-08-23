# Contributing

How a Dotsquares developer proposes a change to a shared agent, skill, wiki page, or prompt in this platform repo. This repo has no restricted-file list of its own beyond the standard secrets rule (see [`.claude/CLAUDE.md`](.claude/CLAUDE.md) §2, §6), but changes here affect every client project built on this framework, so proposals get more scrutiny than a change inside one client repo would.

## Before proposing a change

Adding a new stack entirely (a new agent + wiki page + skill) is its own, larger case — see the ["How do I add a new stack"](docs/FAQ.md) FAQ entry for that specific workflow instead of the steps below.

For everything else — fixing or improving an existing agent, skill, wiki page, prompt, template, or demo — follow the same discipline this platform asks of every other change: **Analyze → Propose → Approve → Implement → Test → Review** (see [`wiki/AI-Workflow-Discipline.md`](wiki/AI-Workflow-Discipline.md)). Concretely, that means:

1. **Identify the actual problem**, not just a symptom — e.g., "the Onboarding Guide references an agent filename that doesn't exist" rather than "the onboarding guide is confusing."
2. **Check whether the fix belongs in one place or several.** This platform has real duplication risk (an agent and its paired skill, a wiki page and a template checklist, multiple prompts covering related ground) — if your change should logically also update a related file, say so in the proposal rather than fixing only the file you happened to open.
3. **Write the proposal down before touching files.** A proposal should state:
   - **What** is changing (which files, what kind of change — content fix, new file, structural change).
   - **Why** — the concrete problem it fixes or the gap it closes, not just "improvement."
   - **Which files are affected**, including any file that references or is referenced by the one you're changing (cross-links, paired agent/skill, an index like `wiki/Home.md` or `prompts/README.md` that may need updating too).
4. **Get it approved** by whoever owns platform-level changes before implementing — this repo's whole value proposition is consistency across 50+ developers' worth of client projects, so a change here is not the same as a change inside one client repo that only affects that one team.

## Making the change

- Match the existing file's structure and tone. A new agent should look structurally like an existing one for a similar kind of stack; a new wiki page should follow the existing pages' "standards + rationale" pattern, not a bare bullet list; a new prompt should follow `prompts/README.md`'s documented template with zero deviation.
- Keep prompts generic and reusable across any client engagement — never bake in one specific client's internal codebase identifiers, class names, or project structure.
- Update every cross-reference a change touches: an index page (`wiki/Home.md`, `prompts/README.md`), a paired agent/skill, and any doc that names the thing you changed.
- Do not fork or duplicate an existing agent/skill/prompt to work around a disagreement with it — raise the disagreement instead (see the FAQ's ["What if I disagree with a standard in this wiki?"](docs/FAQ.md) entry).
- If the change is substantial enough to be worth flagging to already-onboarded client projects, add an entry to [`CHANGELOG.md`](CHANGELOG.md) describing what changed and why.

## Testing and review

- For a change to `demos/`, actually build and test the affected demo project yourself (see that demo's own README for its build/test commands) before calling it done — never claim a build/test passed without having run it.
- For a change to an agent/skill/wiki page/prompt (no compiled code involved), "testing" means re-reading it end-to-end for accuracy and for consistency with whatever it cross-references, and ideally trying it in a real Claude Code session against a project that uses it.
- Review your own diff against [`templates/code-review-checklist.md`](templates/code-review-checklist.md)'s general spirit (correctness, unintended changes, consistency) even though that checklist is written for client-project code review — the same discipline applies to changing the platform itself.

## Related pages

- [docs/FAQ.md](docs/FAQ.md) — including the "How do I add a new stack" and "What if I disagree with a standard" entries referenced above.
- [wiki/AI-Workflow-Discipline.md](wiki/AI-Workflow-Discipline.md)
- [`.claude/CLAUDE.md`](.claude/CLAUDE.md)
