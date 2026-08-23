---
name: code-review
description: >
  Use to run a structured review of a diff or changed files before calling
  work done — the Review step of this platform's core workflow. Trigger
  phrases: "review this code", "code review", "is this ready to merge",
  "check my diff". Works across all supported stacks; delegates to
  code-reviewer (general) and, when security-sensitive surface is present,
  security-reviewer.
---

# Code Review Workflow

This skill runs the Review step of Analyze → Propose → Approve →
Implement → Test → **Review** as a repeatable checklist, not an
open-ended "look it over."

## Step 1 — Scope the review

- Identify exactly what changed: a git diff, a set of files, or a
  just-completed implementation task's output. Review that scope plus
  direct callers/dependencies needed to judge correctness — do not
  expand into an unrelated full-codebase audit.
- Identify the stack(s) involved (ASP.NET Core, EF Core, Blazor, SQL
  Server, SignalR, Power BI, SharePoint, Power Apps, etc.) so the
  matching stack-specific pitfalls apply.
- Note whether the change touches anything security-sensitive: auth,
  external input handling, deserialization, outbound HTTP calls,
  secrets, cryptography, or a new external integration. If so, this
  review must include a security-reviewer pass, not just the general
  checklist.

## Step 2 — Run the general checklist (delegate to code-reviewer)

Invoke the `code-reviewer` agent (or apply its checklist directly) across:

- **Correctness** — does the change do what it claims; edge cases
  covered.
- **Security** — injection, authZ/authN including object-level checks,
  secrets, least-privilege on new external permissions.
- **Nullability** — no null-forgiving operators masking real null paths.
- **Error handling** — failures handled at the right boundary, not
  swallowed or leaking internals; structured logging without secrets/PII.
- **Performance** — N+1 patterns, unbounded result sets, blocking calls
  on async paths.
- **Maintainability** — matches existing project conventions, no
  duplicated logic, reasonable scope.
- **Backward compatibility** — additive API/contract changes, or
  breaking changes called out explicitly with impact.
- **Unintended changes** — diff contains only what the task required.

## Step 3 — Run the security-focused pass when warranted

If Step 1 flagged security-sensitive surface, invoke `security-reviewer`
for an OWASP-mapped pass (injection, authN/authZ, secrets, insecure
deserialization, SSRF, and the other checks in that agent's checklist).
Don't skip this because the general checklist "already covered security
briefly" — the dedicated pass goes deeper on exploitability.

## Step 4 — Stack-specific pitfall check

For each stack touched, spot-check against that stack's known pitfalls
(pull from the matching developer agent, e.g. efcore-developer for
`DbContext` thread-safety/N+1/tracking issues, aspnet-core-developer for
DI captive-dependency issues, signalr-developer for group/backplane
issues) — these are the mistakes generic review misses because they're
framework-specific, not general C# hygiene.

## Step 5 — Report

Structure findings by severity:

- **Blocking** — must fix (security, correctness, broken backward
  compatibility). Work is not done until these are resolved.
- **Should fix** — real issues worth addressing before merge but not
  strictly blocking.
- **Nit** — style/consistency, non-blocking.

Each finding: file/line, why it's a problem, and a concrete suggested
fix — not vague observations. If the change is genuinely clean, say so
plainly rather than manufacturing findings to look thorough.

## Don't
- Don't review scope beyond what changed plus its direct dependencies.
- Don't skip the security-reviewer pass when the change touches auth,
  external input, deserialization, outbound calls, or secrets.
- Don't approve a change you haven't traced against its stated
  requirement.
- Don't fix issues found here yourself unless asked — report them back;
  fixing is the Implement step, already past by the time Review runs.
