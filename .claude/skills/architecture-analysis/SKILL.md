---
name: architecture-analysis
description: >
  Use to explain how a flow or feature works across layers or projects in
  a .NET solution without over-reading the codebase. Trigger phrases: "how
  does X work", "explain the architecture of X", "what calls Y", "trace
  this end to end". Works across any supported stack; the goal is minimum
  reading for maximum confidence in the answer, using existing project
  documentation before re-deriving architecture from source. Delegates the
  actual trace/explanation to the `architecture-analyst` agent.
---

# Architecture Analysis Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

Explaining a flow correctly with the fewest files read is the actual
skill here — this is the discipline that keeps the platform's minimum-
context principle real for open-ended "how does this work" questions,
which are otherwise the easiest way to blow a context budget.

## Step 1 — Pin down the actual entry point

- Translate the question into a concrete starting point: a specific HTTP
  route, SignalR hub method, background job trigger, page load, or
  component render — not "the whole feature."
- If genuinely ambiguous which entry point the question means, state the
  assumption explicitly (or ask) before tracing, rather than tracing
  every plausible interpretation.

## Step 2 — Check for existing documentation first

- Look for project-level architecture docs before reading source: a
  `Notes/*-Documentation.md` convention, `wiki/` pages, or a project
  README describing the flow.
- If found, use it as the starting narrative, then spot-check it against
  a quick read of the actual entry point and one or two key hops — don't
  re-derive the whole architecture from scratch when a doc already
  covers it accurately.
- If the doc looks stale (references a class/method that's since moved
  or is described inconsistently with what a spot-check shows), say so
  explicitly rather than silently trusting or silently ignoring it.

## Step 3 — Trace forward, one hop at a time

- From the entry point, follow the call chain by targeted search
  (method/route/event name), not by reading whole files start to finish.
- Read only the specific method body needed to see what it calls next;
  move to the next hop rather than reading the rest of that file "while
  you're in there."
- When a call crosses a DI-registered interface, resolve the concrete
  implementation via its registration once, note it, and don't
  re-resolve it again later in the same trace.
- When the flow crosses a process/project boundary (an app enqueuing
  work a separate worker picks up, an HTTP call to another service),
  state the boundary explicitly; don't assume synchronous behavior
  across an async/queued/networked hop.
- Stop tracing once you've reached the point that actually answers the
  question (a data store write, an external API call, a rendered
  response, a returned value) — don't keep tracing past that just to be
  thorough.

## Step 4 — Handle scope blowouts explicitly

- If fully resolving one hop would require reading deep into
  third-party/framework internals not owned by the project, stop there
  and describe the documented/expected behavior of that boundary instead
  of exhaustively reading library source.
- If the question turns out to be broader than initially scoped (spans
  many independent flows), say so and ask whether to narrow it rather
  than silently attempting an exhaustive trace of everything.

## Step 5 — Report

- Lead with a direct 2-4 sentence answer to the question.
- Follow with a numbered step-by-step trace: file, class/method, what
  happens, for each hop that matters.
- Call out meaningful branch points (conditionals that change the flow)
  relevant to the question — skip irrelevant branches.
- Note anything surprising found along the way (an apparently-synchronous
  call that's actually fire-and-forget, validation happening later than
  expected).
- State explicitly if existing documentation was used and whether it
  matched the code.

## Don't
- Don't read an entire codebase/project "to be safe" — stop once the
  question is answered with confidence.
- Don't propose fixes or changes — this is an explanation task; note a
  suspected bug if spotted, but don't fix it here.
- Don't re-read a file/section whose relevant content is already
  established earlier in the same trace.
- Don't present an unconfirmed guess as fact — say what wasn't
  confirmed.
