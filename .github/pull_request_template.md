<!--
This is an internal Dotsquares platform repo (see LICENSE) — no external contributions are
expected here, but the same Analyze → Propose → Approve → Implement → Test → Review discipline
this platform asks of every AI-assisted change (see wiki/AI-Workflow-Discipline.md) applies to
any internal PR against it too.
-->

## Summary

What changed, and why. State the actual problem this fixes or the gap it closes — not just
what files moved.

## Files affected

List every file this PR touches, and call out any **cross-references** it also updates
(per `CONTRIBUTING.md`'s guidance) — e.g. a paired agent/skill, an index page
(`wiki/Home.md`, `prompts/README.md`), or a doc that names the thing you changed:

-

## How this was tested

- **If this touches `demos/`:** the build/test commands actually run, and their result.

  ```
  dotnet build <solution>
  dotnet test <solution>
  ```

- **If this touches an agent/skill/wiki page/prompt/template (no compiled code):** confirm
  you re-read it end-to-end for accuracy and consistency with what it cross-references, and
  note whether you tried it in a real Claude Code session.

## Review checklist

- [ ] Reviewed this diff against [`templates/code-review-checklist.md`](../templates/code-review-checklist.md)'s spirit (correctness, security, unintended changes, backward compatibility — even though that checklist is written for client-project code, the same discipline applies to changing the platform itself).
- [ ] No secrets, connection strings, tenant IDs, or credentials introduced anywhere in the diff, including in demo `appsettings.json` files.
- [ ] Only the files necessary for this change are included — no incidental formatting/reordering churn.
- [ ] Any cross-referencing file (paired agent/skill, index page, related wiki page) was updated to match, or confirmed not to need updating.
- [ ] If this change is substantial enough to flag to already-onboarded client projects, an entry was added to [`CHANGELOG.md`](../CHANGELOG.md).

## Additional context

Anything else a reviewer needs — related issue, prior discussion, follow-up work intentionally left out of scope.
