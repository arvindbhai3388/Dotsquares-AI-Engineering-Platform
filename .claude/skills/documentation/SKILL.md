---
name: documentation
description: >
  Use to create or update project documentation after a source change, or
  to check whether existing docs are still accurate. Trigger phrases:
  "update the docs", "document this change", "is the project doc still
  accurate", "write up this architecture". Keeps per-project documentation
  in sync with code rather than letting it drift, without turning every
  change into a documentation project.
---

# Documentation Workflow

Documentation drift — a doc that describes how something used to work —
is worse than no documentation, because it's actively misleading and
consumed with false confidence (including by `architecture-analyst`,
which checks existing docs first). This skill keeps updates small, honest,
and tied to actual changes.

## Step 1 — Determine if documentation update is warranted

Not every change needs a doc update. Update documentation when a change:

- Alters a described architecture/flow (a new layer, a changed call
  path, a new external integration).
- Adds or removes a public contract (API endpoint, SignalR hub method,
  connector operation, exported type) that a doc describes or should
  describe.
- Introduces a new project, service, or major component.
- Changes a documented configuration/setup requirement.

Skip a doc update for purely internal refactors that don't change any
described behavior, contract, or setup step — don't manufacture
documentation churn for its own sake.

## Step 2 — Locate the right document

- Follow the project's existing documentation convention rather than
  inventing a new location or format. Common convention in this
  ecosystem: a `Notes/<Project>-Project-Documentation.md` file per
  project, or a `wiki/` page per architecture area — check what the
  target project/repo already uses before creating something new.
- If genuinely no documentation exists yet for the area being changed,
  confirm with the user whether a new doc should be created (and where)
  rather than assuming a format and location.
- Search narrowly for the existing doc (the project's own directory)
  before concluding none exists.

## Step 3 — Update precisely

- Read the existing doc's relevant section fully before editing — match
  its existing structure, heading style, and level of detail; don't
  introduce a wildly different style for one section.
- Update only what the code change actually affects. Don't take the
  opportunity to rewrite unrelated sections, even if they look
  improvable — that's a separate, explicitly-requested task.
- If the change makes a section of the doc actively wrong (not just
  incomplete), fix that section precisely rather than appending a
  contradicting note below the stale text.
- Keep examples/diagrams/flow descriptions in the doc consistent with
  what a quick trace of the actual (post-change) code confirms — don't
  document intended behavior that differs from what was actually
  implemented.

## Step 4 — Cross-check accuracy, not just presence

- After updating, verify the new doc text against the actual code it
  describes (file names, method names, actual flow order) — a doc update
  that's syntactically plausible but factually wrong is the same drift
  problem this skill exists to prevent.
- If updating documentation surfaces that an *unrelated* existing section
  is already stale, flag it to the user rather than silently fixing (out
  of the current change's scope) or silently ignoring it.

## Step 5 — Report

State exactly which file(s) were updated and a one-line summary of what
changed in each — enough for the user to spot-check without re-reading
the whole document.

## Do
- Match the existing documentation convention and format exactly.
- Update only what the change actually affects.
- Verify updated text against the real, current code.

## Don't
- Don't create a new documentation format/location when an established
  one already exists for this project.
- Don't rewrite unrelated sections while updating one.
- Don't document intended/aspirational behavior that doesn't match what
  was actually implemented.
- Don't skip documentation updates for changes that alter a described
  contract or architecture.
