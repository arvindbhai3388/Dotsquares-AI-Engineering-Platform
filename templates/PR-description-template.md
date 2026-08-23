<!--
Template usage: Copy this into the client repo's PR template location
(e.g. .github/PULL_REQUEST_TEMPLATE.md) or paste it into a PR description by hand.
Fill in every <PLACEHOLDER>. This template exists to enforce the framework's
analyze → propose → approve → implement → test → review discipline — a PR that
skips the "Why" or "Testing Done" sections did not follow that discipline.
Delete this comment block before submitting.
-->

## Summary

<PLACEHOLDER — one or two sentences: what does this PR do, in plain language.>

## Why

<PLACEHOLDER — the problem/ticket this addresses and the root cause, not just the symptom.
Link the ticket: `<TICKET_LINK>`>

## What Changed

<PLACEHOLDER — bullet list of the actual changes, by file/module if the diff spans more than
one. Call out anything intentionally left out of scope.>

- `<file/module>` — `<what changed and why>`
- `<file/module>` — `<what changed and why>`

## Approach Considered / Rejected

<PLACEHOLDER — optional, but required for any non-trivial or architectural change: what
alternative approach(es) were considered and why this one was chosen instead. Skip this
section for small, obvious fixes.>

## Testing Done

- [ ] Unit tests added/updated for the new behavior (list them): `<PLACEHOLDER>`
- [ ] Existing test suite run and passing: `<BUILD/TEST COMMAND USED>`
- [ ] Manually verified: `<PLACEHOLDER — what you clicked/called and what you saw>`
- [ ] Edge cases considered: success / validation failure / error / authorization /
      cancellation (strike through any that don't apply and say why)

## Screenshots / Recordings (UI changes only)

<PLACEHOLDER — before/after screenshots or a short recording. Delete this section entirely
for non-UI changes.>

## Database / Migration Impact

<PLACEHOLDER — "None" if not applicable. Otherwise: migration name, whether it's
backward-compatible with the currently deployed app version, and the rollback approach.>

## Checklist

- [ ] Smallest correct change — no unrelated refactors, renames, or dependency upgrades
- [ ] Matches existing project patterns and style
- [ ] No secrets, connection strings, tokens, or credentials in the diff
- [ ] No restricted/config files modified without explicit approval
- [ ] Backward compatible, or breaking changes are explicitly called out above
- [ ] Nullability, error handling, and cancellation paths considered
- [ ] Logging added/updated without logging secrets or unnecessary personal data
- [ ] Ran `code-review-checklist.md` against this diff before requesting review

## Reviewer Notes

<PLACEHOLDER — anything you want the reviewer to pay particular attention to.>
