---
description: Fast Mode - a small, low-risk fix. Investigation is shortened, but Test and Review are never skipped.
argument-hint: <short description of the fix>
---

Fast Mode for a small, low-risk change: $ARGUMENTS

This is explicitly **not** the full `new-feature` ceremony — only use Fast Mode for something
genuinely small and low-risk (a typo, a copy change, an obvious one-line bug fix, a trivial
config default). If what's actually being asked turns out to be bigger than that once you look
at the code, say so and switch to `/new-feature` instead of forcing it through Fast Mode anyway.

## Still required, never skipped even in Fast Mode

1. Locate the actual code and confirm you understand the real cause — a "small" fix built on a
   wrong assumption about the code is still wrong, regardless of how small the diff looks.
2. State what you're about to change, in one or two sentences, before changing it.
3. Implement the smallest correct change.
4. Test — write/update a test if one plausibly should exist for this change; run the actual
   test suite for real (see the `unit-testing` skill).
5. Review — at minimum self-check against `code-reviewer`'s standing checklist before calling it
   done.

## What's actually skipped in Fast Mode (only this, nothing else)

The full multi-section Plan document, and any per-step approval beyond one initial
confirmation — Fast Mode defaults to `wiki/AI-Workflow-Discipline.md`'s Streamlined mode rather
than making it something to opt into separately.

## Escalate if reality doesn't match "small and low-risk"

If this task turns out to touch a database schema, authentication, or any other
production-risk surface once you're actually in the code, stop and say so — `/safefeature` is
the right discipline for that, not this one.
