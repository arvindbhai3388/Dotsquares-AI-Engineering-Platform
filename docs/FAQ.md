# FAQ

Common questions from developers adopting this framework. If your question isn't here, check [wiki/Home.md](../wiki/Home.md) for the full index, or ask your team lead — and consider whether the answer should be added back here for the next person.

---

**Q: Can AI commit code for me, or open a pull request on my behalf?**

No, not by default and not silently. This platform's standing rule — repeated in both this repo's `CLAUDE.md` and every client project template — is that Claude never runs `git commit`, `git push`, or opens a pull request unless you explicitly ask for that specific action in that turn. Finishing an implementation is never by itself treated as a reason to commit. This exists so a human always makes the final call on what enters version control, and so an approval to commit once doesn't quietly generalize to every later change in the same session.

---

**Q: What if the agent picks the wrong stack or the wrong approach for a feature?**

This is exactly what the **Propose → Approve** gate in the [AI Workflow Discipline](../wiki/AI-Workflow-Discipline.md) exists to catch — before any file is written, the agent should state its planned approach, and you're expected to read it and redirect if it's wrong, not just skim to the diff. If an agent jumps straight to implementation without proposing first, that's a process failure worth flagging (and worth checking whether the corresponding skill's Approve gate is actually configured correctly for that project). If a wrong approach did get implemented, the fix is the same as fixing any wrong code: revert or correct it through the same disciplined cycle, and consider whether the original request was ambiguous enough that the mistake was reasonable given what was asked.

---

**Q: How do I add a new stack (or a new version of an existing one) to this framework?**

Treat it as its own small project, following the same discipline as any other change to this repo:

1. Propose the addition explicitly — which stack, why it's needed, and roughly what a corresponding agent/skill/wiki page should cover — rather than silently dropping files into `.claude/agents/` or `wiki/`.
2. Add a new stack-specific agent under `.claude/agents/` modeled on an existing one for a similar kind of stack (e.g., a new front-end framework agent should look structurally like the existing `blazor-developer`, not be invented from scratch).
3. Add a corresponding wiki page under `wiki/` following the existing pages' structure (standards + rationale, not just a bullet list), and cross-link it from [wiki/Home.md](../wiki/Home.md) and [wiki/Architecture-Overview.md](../wiki/Architecture-Overview.md)'s layer mapping.
4. If the stack needs its own recurring workflow (e.g., a migration process, a scaffolding step), add a skill under `.claude/skills/`.
5. Consider whether a demo project under `demos/` would help validate the new stack's agent/skill end-to-end — not mandatory for every addition, but valuable for anything substantial.

Do not add a new stack by copy-pasting an unrelated agent and lightly editing it without actually understanding that stack's real constraints — a shallow agent definition produces confidently wrong advice, which is worse than no agent at all.

---

**Q: What if a client project already has its own conventions that conflict with these standards?**

The client project's own established conventions win for that project. This framework's wiki pages are the **default** a project falls back on when it hasn't already decided something for itself — they are not a mandate to refactor an existing, working, differently-styled codebase into alignment with this wiki. Concretely: if a client's existing ASP.NET MVC codebase names things differently than [C# Coding Standards](../wiki/Coding-Standards-CSharp.md) prefers, match the existing codebase's convention for changes within that codebase, don't introduce a second style. If you think a client's existing convention is actively harmful (e.g., no parameterization on SQL queries), that's a security/correctness finding worth raising explicitly with the client/team lead — not something to silently override mid-ticket, and not something to silently leave in place either.

---

**Q: Does this framework apply to demo projects and to real client projects the same way?**

Mostly, with one meaningful difference: demo projects under `demos/` in this repo are explicitly required to never connect to real external tenants (SharePoint, Power BI, Power Apps/Dataverse) — they use interface-based mock/stub implementations behind the same contract a real integration would use. Client projects, by contrast, do connect to real tenants and carry the full weight of [Security Guidelines](Security-Guidelines.md) around real credentials. The coding standards themselves (naming, DI lifetimes, async rules, etc.) apply identically in both.

---

**Q: What test framework should I use for a new client project?**

Match whatever the project has already established. If a project genuinely has no test project yet, xUnit + Moq is this platform's default recommendation for new .NET (SDK-style) test projects — it's the combination used across this platform's own demo projects and most current agent/skill tooling assumes it when scaffolding a new test project. MSTest remains appropriate for legacy ASP.NET Framework projects that already standardized on it; don't introduce a second test framework into a project that already has one just because it's the platform default.

---

**Q: How much should I trust an agent's claim that "the build passed" or "tests are passing"?**

Trust it provisionally, verify it directly for anything you're about to consider done. The [Test](../wiki/AI-Workflow-Discipline.md) and Review steps exist specifically because a plausible-sounding claim of success is not the same thing as a build that was actually run — an agent should never claim a build/test succeeded without having actually executed it, but mistakes and stale assumptions happen. Running the actual build/test command yourself, at minimum periodically, is part of building calibrated trust in the workflow rather than a sign the workflow isn't working.

---

**Q: Where do secrets go if I need one for local development?**

Never in a checked-in file. Use `appsettings.Development.json` (confirm it's gitignored on that specific project) or .NET user-secrets. See [Security Guidelines](Security-Guidelines.md) for the full policy and where secrets belong in each environment (local, CI/CD, production).

---

**Q: Can I skip the Test-First step if I'm just making a trivial change?**

For a genuinely trivial change (a typo fix, a copy change, a one-line config default), yes — the discipline scales down for low-risk changes. For anything touching actual business logic, branching, or an existing behavior other code depends on, write or update the test first even if the change itself looks small; the failure modes described in [AI Workflow Discipline](../wiki/AI-Workflow-Discipline.md) (silent regressions, untested edge cases) don't correlate with how large a diff looks.

---

**Q: What if I disagree with a standard in this wiki?**

Raise it with the framework's maintainers rather than silently deviating on your own project, and definitely rather than editing the wiki page unilaterally to match what you did. A standard that's wrong for a good reason should be fixed for everyone; a standard that's inconvenient for one specific case is usually better handled as a documented, deliberate per-project exception (see the conflicting-conventions answer above) than a change to the shared baseline.

---

**Q: Do I need to read every wiki page before I can start working?**

No — see the [Onboarding Guide](../wiki/Onboarding-Guide.md) for a paced first-two-weeks reading plan. Read the foundational pages ([Architecture Overview](../wiki/Architecture-Overview.md), [AI Workflow Discipline](../wiki/AI-Workflow-Discipline.md)) and [C# Coding Standards](../wiki/Coding-Standards-CSharp.md) up front; read stack-specific pages as you're actually assigned to work touching that stack.

---

**Q: A skill or agent in this repo seems to contradict something in the wiki — which one is right?**

Treat this as a bug worth reporting, not a judgment call to make silently. Agents and skills are supposed to be an executable encoding of the wiki's standards; a real contradiction between them means one of the two documents is stale. Flag it to the framework maintainers rather than picking whichever one is more convenient for your current task.

---

**Q: How do I pull an update from this platform into an already-onboarded client project?**

There's no live link to pull from — a client project adopted this framework by *copying* `templates/CLAUDE-*.md` (and, for permissions/MCP, `templates/permissions-baseline.json`/`templates/mcp-baseline.json`) into its own repo, so an update here doesn't automatically reach any client project. To pull in a later change:

1. Check the root [`CHANGELOG.md`](../CHANGELOG.md) in this repo for what's changed since the version the client project last copied from.
2. Diff the client project's own `CLAUDE.md` (and `.claude/settings.json`/`.mcp.json` if it copied those too) against the current `templates/CLAUDE-full.md` or `templates/CLAUDE-minimal.md` (whichever it started from) to see exactly what moved.
3. Manually re-apply whatever's relevant to that project — don't blindly overwrite the client's filled-in `CLAUDE.md`, since it contains that project's own project-specific sections (restricted files, actual stack/versions, build/test commands) alongside the shared baseline content, and a naive overwrite would wipe those out.
4. Treat this the same as any other change to a client repo: propose what you're about to bring in and why, get it approved, then apply it — not a silent background sync.

---

## Related pages

- [wiki/Home.md](../wiki/Home.md)
- [wiki/AI-Workflow-Discipline.md](../wiki/AI-Workflow-Discipline.md)
- [Security Guidelines](Security-Guidelines.md)
- [Getting Started](Getting-Started.md)
