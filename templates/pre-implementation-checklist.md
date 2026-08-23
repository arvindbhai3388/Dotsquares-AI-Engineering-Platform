# Pre-Implementation Checklist

> **Template usage:** Run through this before writing any implementation code, as the "Plan"
> step of the `Understand → Locate → Inspect → Plan → Test-First → Implement → Validate →
> Review` workflow (see `templates/CLAUDE-full.md` §5) — the concrete, day-to-day expansion
> of this platform's core `Analyze → Propose → Approve → Implement → Test → Review`
> discipline (`wiki/AI-Workflow-Discipline.md`). Copy relevant items into a PR/ticket comment
> if the team wants a paper trail, or just use it as a mental gate. Applies to any stack in
> this platform.

## 1. Understanding

- [ ] The requested behavior and its acceptance criteria are clear (not assumed).
- [ ] The ticket/request has been read in full — no requirement guessed or skipped.
- [ ] Ambiguities have been resolved by asking, not by picking the most convenient
      interpretation.
- [ ] The relevant project/module has been identified before searching further.

## 2. Root Cause (bug fixes)

- [ ] The actual root cause is identified — not just the symptom being patched.
- [ ] It's clear why the bug wasn't caught earlier (missing test, edge case, race
      condition, bad assumption) so the fix addresses that, not just today's repro.
- [ ] Confirmed the same root cause doesn't appear in other places that also need fixing
      (or, if it does, that's called out explicitly rather than silently fixed everywhere).

## 3. Existing Patterns

- [ ] Searched for an existing service/helper/utility that already solves this or something
      close to it — reuse over reinvention.
- [ ] Reviewed how similar features/endpoints/components are implemented in this codebase,
      and plan to match that style rather than introducing a new one.
- [ ] Confirmed the target stack's idioms (DI lifetime, async patterns, error-handling
      shape, options binding, etc.) match what the rest of the project already does.

## 4. Smallest Safe Solution

- [ ] The proposed change is the smallest one that correctly satisfies the requirement —
      not the most "complete" or "future-proof" one.
- [ ] No unrelated refactors, renames, formatting changes, or dependency upgrades are bundled
      into this change.
- [ ] No new abstraction/interface/pattern is being introduced unless the existing code
      genuinely can't express the change without one.
- [ ] If a new dependency is being considered, an existing project dependency was ruled out
      first, and the addition will be called out explicitly (see §6 of `CLAUDE-full.md`).

## 5. Backward Compatibility & Side Effects

- [ ] Existing public API fields, response shapes, and status codes are preserved, or the
      break is explicit and intentional.
- [ ] Any database schema change has an expand/contract path if the app may run overlapping
      versions during deploy.
- [ ] Downstream consumers (other services, background jobs, scheduled tasks, other
      developers' in-flight branches) of the code being changed have been considered.
- [ ] No behavior outside the scope of the ticket will change as a side effect.

## 6. Security & Data

- [ ] No secrets, connection strings, tokens, or credentials will be introduced, logged, or
      hardcoded.
- [ ] External/user input paths touched by this change will be validated and authorized.
- [ ] Any restricted/config file this task seems to require has been identified as
      restricted — plan uses strongly typed options/DI instead of opening it directly.

## 7. Test-First Readiness

- [ ] The test project and framework for this code are known (or confirmed with the team
      lead if none exists yet).
- [ ] The failing test(s) to write first are identified — what behavior they pin down, and
      which success/failure/edge cases they need to cover.

## 8. Scope Confirmation

- [ ] The full list of files expected to change has been sketched out, and it matches "the
      smallest correct change" — if it's larger than expected, the reason why is understood.
- [ ] Ready to proceed to Test-First / Implement.
