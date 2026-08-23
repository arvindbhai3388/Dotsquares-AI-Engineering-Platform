# Document a Cross-Service Request/Data Flow

**Category:** Architecture & Planning
**Use when:** Onboarding to or debugging a flow that spans more than one codebase/service.

## Prompt

Document, end-to-end, how the flow described below actually works across the codebase — do not guess or describe how it "should" work; trace the real code path. This is a read-only documentation task: do not modify any source files. Search only the projects/services actually involved in this flow rather than scanning the whole repository.

Produce a document with these sections:

1. **Entry point** — the exact controller/endpoint, message handler, scheduled job, or UI action that initiates the flow, with file and method names.
2. **Step-by-step trace** — each hop the request/data takes across services, processes, or projects, in order. For each step, name the responsible class/method, what it does to the data (validation, transformation, persistence, enqueue/dequeue), and how it hands off to the next step (direct call, HTTP, message queue, shared database table, file, reflection-loaded plugin, etc.).
3. **State and storage touchpoints** — every table, queue, cache, or external API the flow reads from or writes to, and at which step.
4. **Sequence diagram** — a Mermaid `sequenceDiagram` block showing the participants (services/components) and the ordered messages/calls between them, matching the trace in section 2 exactly (do not simplify away real intermediate hops).
5. **Failure and retry behavior** — what happens at each step if it fails (exception handling, retry logic, dead-lettering, partial-state cleanup), and whether the flow is idempotent if retried.
6. **Known gaps or inconsistencies** — anything you find in the actual code that looks like it disagrees with the intended design, is undocumented, or looks fragile (e.g., missing error handling, an assumption baked into a magic string/config value) — flag these clearly as observations, not as things you are fixing.
7. **Where to make changes** — if someone needed to alter behavior at a specific point in this flow, name the most likely file(s) to start from and why.

Keep the trace grounded in what the code actually does; where you are inferring behavior rather than having read it directly, say so explicitly.
