# Onboarding Guide

A new Dotsquares developer's first two weeks with the AI Engineering Platform. The goal by the end of week two is not memorizing every standard in this wiki, but knowing where to look and which agent/skill to reach for, so day-to-day work develops the habit of using the framework rather than working around it.

## Before day one

- Get Claude Code installed and authenticated — see [docs/Claude-Code-Setup.md](../docs/Claude-Code-Setup.md).
- Confirm access to whichever client project repo(s) you'll be working in, and to this platform repo (`Dotsquares-AI-Engineering-Platform`) itself, since client repos reference it but don't vendor a copy of it.

## Days 1–2: read before you run anything

Read, in this order:

1. Root [`.claude/CLAUDE.md`](../.claude/CLAUDE.md) of this repo — the platform's own rules, which take precedence over everything else here.
2. [`README.md`](../README.md) — the one-page shape of the whole repository.
3. [Architecture Overview](Architecture-Overview.md) — how a typical Dotsquares client solution is layered and which stack sits where.
4. [AI Workflow Discipline](AI-Workflow-Discipline.md) — the analyze → propose → approve → implement → test → review sequence every agent and skill is built around. This is the single most important page in the wiki; everything else is detail under this discipline.
5. [docs/Getting-Started.md](../docs/Getting-Started.md) — walks through cloning an actual client project and running your first real Claude Code session against it.

Do this reading with a specific client project in mind if you've already been assigned one — it makes the architecture and workflow material concrete instead of abstract.

## Days 3–5: coding standards for your stack(s)

You don't need every stack page — read the ones matching the client project(s) you're assigned to, plus [C# Coding Standards](Coding-Standards-CSharp.md) unconditionally (it applies everywhere):

- Web app on ASP.NET Core/MVC/Razor Pages → [Coding Standards — ASP.NET Core, MVC & Razor Pages](Coding-Standards-AspNetCore-MVC-Razor.md)
- Blazor front end → [Coding Standards — Blazor](Coding-Standards-Blazor.md)
- CMS-driven site → [Umbraco Guidelines](Umbraco-Guidelines.md)
- Any project touching a database → [EF Core Guidelines](EFCore-Guidelines.md) and [SQL Server Guidelines](SQL-Server-Guidelines.md)
- Real-time features or Blazor Server → [SignalR Guidelines](SignalR-Guidelines.md)
- Analytics/reporting embedded in the app → [Power BI Integration](PowerBI-Integration.md)
- Document/content integration with Microsoft 365 → [SharePoint Integration](SharePoint-Integration.md)
- Power Platform apps or connectors → [Power Apps Integration](PowerApps-Integration.md)

While reading, keep the project's **own** `CLAUDE.md`/conventions open alongside — where the two disagree, the client project's own established conventions win (see [FAQ](../docs/FAQ.md)). This wiki is the default when a project hasn't already decided something for itself, not an override of what a project has already decided.

## Days 6–8: run the discipline on a small, real task

Pick (or ask your lead for) a small, low-risk, real ticket — a bug fix or a small enhancement, not a brand-new feature — and run the full cycle deliberately, narrating each step to yourself:

1. **Analyze** — before writing a single prompt asking for a fix, read the actual affected code yourself first, so you can sanity-check what the agent tells you against what you already saw.
2. **Propose** — ask the appropriate stack agent (see the table below) to investigate and propose an approach; read its proposal fully before responding, don't skim to the code.
3. **Approve** — explicitly approve, redirect, or ask a clarifying question. Get comfortable saying "no, do X instead" here — this is the cheapest point in the whole cycle to redirect.
4. **Implement** — let the agent make the change.
5. **Test** — confirm tests were actually written/run, not just claimed. Run the build/test command yourself once, even if the agent already reports having done so, until you trust the pattern.
6. **Review** — read the diff yourself, and separately run `/code-review` (or the equivalent skill) against it, and compare what you noticed to what it noticed.

Doing this once, deliberately and slowly, teaches the discipline far better than reading about it a second time.

## Days 9–10: agents and skills reference

Get familiar with which agent or skill to reach for. As a rule of thumb:

| You want to... | Reach for... |
|---|---|
| Understand how an existing feature/flow works before touching it | `architecture-analyst` agent, or `/architecture-analysis` |
| Implement a change in a specific stack | The matching stack agent (`aspnet-core-developer`, `mvc-developer`, `razor-pages-developer`, `blazor-developer`, `umbraco-developer`, `efcore-developer`, `signalr-developer`, `sql-server-developer`, `sharepoint-developer`, `powerbi-developer`, `powerapps-developer`) |
| Write or update tests before/after implementing | `unit-test-writer` agent, or `/unit-testing` |
| Get a second opinion on a diff before calling it done | `code-reviewer` agent, or `/code-review` |
| Check for security issues (auth, secrets, injection) | `security-reviewer` agent |
| Do a final build/test pass before calling a task complete | `build-validator` agent, or `/build-validation` |
| Update project documentation after a change | `/documentation` |

When a task spans more than one stack/layer, start with `architecture-analyst` (or `/architecture-analysis`) to scope which agents/layers are actually involved before diving into implementation with a single stack agent — this mirrors the Analyze step of the [AI Workflow Discipline](AI-Workflow-Discipline.md) and avoids a stack agent confidently implementing a change that ignores a layer it wasn't scoped to look at.

## End of week two: self-check

By the end of week two you should be able to answer, without looking anything up:

- What are the six steps of the workflow discipline, and what does each one prevent?
- Which layer does the project(s) I'm working on put its business logic in, and why does it matter that it's not in the controller/component?
- If I need to add a database column, what's the actual sequence of migrations, and why isn't it just one `RENAME`?
- If a client project's existing conventions conflict with something in this wiki, which one wins, and where is that documented?

If any of those aren't automatic yet, revisit the relevant page before taking on larger, higher-stakes work — the investment here compounds across every project you'll touch afterward.

## Related pages

- [AI Workflow Discipline](AI-Workflow-Discipline.md)
- [Architecture Overview](Architecture-Overview.md)
- [docs/Getting-Started.md](../docs/Getting-Started.md)
- [docs/FAQ.md](../docs/FAQ.md)
