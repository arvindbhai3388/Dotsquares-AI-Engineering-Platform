# Security Policy

This is an internal Dotsquares platform repository (see [`LICENSE`](LICENSE)) — not a public
open-source project — but the same care applies to reporting a security concern here as
anywhere else.

## What counts as a security concern here

This repo is mostly a framework/template (`.claude/agents`, `.claude/skills`, `templates/`,
`wiki/`, `docs/`, `prompts/`) plus three standalone demo projects under `demos/`. Report it as a
security concern if you find:

- A prompt, agent, or skill that could produce insecure guidance if followed literally (e.g.
  a code pattern that skips input validation, weakens authorization, or suggests logging a
  secret).
- An accidentally-committed secret, connection string, tenant ID, API key, or credential
  anywhere in the repo, including in a demo's `appsettings.json` or git history.
- A real vulnerability in one of the three demo projects under `demos/` (e.g. an injection
  flaw, a broken auth check, an unsafe deserialization path) — see the note below on what is
  *not* a demo vulnerability.
- Anything in `docs/Security-Guidelines.md` or the [restricted-files pattern](docs/Security-Guidelines.md#the-restricted-files-pattern)
  that turns out to be wrong or incomplete in a way that would let a real secret slip through.

## What is *not* a vulnerability

Per [`.claude/CLAUDE.md`](.claude/CLAUDE.md) §4, the three demo projects intentionally **never**
connect to a real external tenant — no real SharePoint site, Power BI workspace, or Power Apps
environment, and no real Azure AD app registration. Every Microsoft 365/Power Platform
integration in the demos is a mock/stub implementation behind an interface, clearly documented
as such (see, e.g., Demo3's "mock now / real later" seam in its `README.md`).

If a demo appears to make a real network call to SharePoint, Power BI, Power Apps, or any other
external Microsoft 365/Azure service, that is itself a bug worth reporting — it contradicts the
demo's own design goal — not expected or acceptable behavior.

## How to report

Do not open a public issue for a suspected secret leak or an exploitable vulnerability — treat
those as sensitive until a maintainer has assessed them. Contact the framework's maintainers
directly (the same escalation path described in [`CONTRIBUTING.md`](CONTRIBUTING.md) and
[`docs/FAQ.md`](docs/FAQ.md) for platform-standard disagreements) rather than filing a normal
`bug_report` issue.

For anything that is clearly *not* sensitive on its own — e.g. a suggestion to tighten a
least-privilege scope example in `docs/Security-Guidelines.md`, or to add a missing check to
`templates/code-review-checklist.md` — a normal [improvement issue](.github/ISSUE_TEMPLATE/improvement.md)
is fine.

## What to include

- Which file(s)/agent/skill/prompt/demo are affected.
- What the concern is and, if applicable, how to reproduce it.
- If you found a committed secret: name the file and location, but do not re-post the secret
  value itself anywhere (issue, PR, commit message, chat) — treat it as compromised and flag it
  for rotation through the appropriate channel instead.
