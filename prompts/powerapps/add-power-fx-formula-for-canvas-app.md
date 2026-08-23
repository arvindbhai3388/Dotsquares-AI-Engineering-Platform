# Write and Debug a Power Fx Formula for a Canvas App Control

**Category:** Power Apps / Power Platform
**Use when:** A canvas app control needs non-trivial calculated/conditional behavior.

## Prompt

Write (or debug, if I paste an existing broken/inefficient one) a Power Fx formula for the control and property I specify (e.g. a label's `Text`, a container's `Visible`, a gallery's `Items`, a button's `DisplayMode`, or a form's `Item`). Ask me for the exact behavior expected in each state/branch before writing the formula, rather than guessing intent from a vague description.

When producing the formula:
- Prefer `With()` to name intermediate calculations once instead of repeating the same sub-expression multiple times across nested `If()`/`Switch()` branches -- this is both more readable and avoids redundant re-evaluation.
- Use `Switch()` instead of nested `If()` when branching on a single value's discrete states, and `If()` with clear boolean conditions otherwise; avoid deeply nested ternary-style `If()` chains that are hard to audit.
- If the formula reads from a data source (Dataverse, SQL, SharePoint), check whether the expression as written will delegate; if it won't (e.g. using `Sort` on a non-delegable computed column, or a `Filter` combining functions the connector can't push down), say so explicitly and propose a delegable rewrite or a documented, deliberate exception (with the row-limit tradeoff stated).
- Handle blank/error states defensively: wrap external/lookup-dependent expressions with `IfError()` or `Coalesce()` where a blank or erroring related record could otherwise throw a red error banner to the user.
- For anything performance-sensitive (formulas that run on every `OnChange`/gallery re-render), avoid repeated data-source calls inside the formula (e.g. calling `LookUp` multiple times for the same key) -- collect once into a variable/collection in `OnVisible`/`OnStart` if that matches this app's existing pattern.
- Match the naming and structure conventions already used in this app's other formulas (inspect a couple of existing screens/controls first if the source is available as `.pa.yaml`).

Explain the formula in plain language after presenting it (what each branch does and why), so it can be code-reviewed by someone who didn't write it. If this is a fix to a reported bug, first reproduce why the existing formula produces the wrong result before proposing the replacement, rather than rewriting from scratch.
