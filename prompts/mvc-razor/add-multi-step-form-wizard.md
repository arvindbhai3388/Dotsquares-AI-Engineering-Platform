# Implement a Multi-Step Form Wizard

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a single form is too large and needs to be split into a guided sequence.

## Prompt

The form I point you to has grown too large for a single page and needs to become a guided multi-step wizard, with state persisted across steps until final submission. Before implementing, propose the step breakdown (which fields belong to which step) and, critically, where per-step state will live between requests -- session, TempData (only viable for very short flows since it doesn't survive more than the next redirect without `Keep()`), a server-side draft record persisted to the database, or a single view model carried across steps via hidden fields -- and get my approval on the approach, since this decision affects concurrency behavior (what happens if the user opens the wizard in two tabs) and data loss risk (what happens if they abandon it halfway).

Design one view model per step plus an aggregate model representing the full wizard state, with each step's model validated independently (`ModelState.IsValid` per step, not deferred to the final step) so users get feedback as they go rather than at the end. Implement navigation actions (Next/Back/Cancel) that persist the current step's data before moving, and that reload previously entered data correctly when the user navigates back -- verify Back doesn't silently discard already-entered data. Handle the case where a user jumps directly to a later step's URL without completing earlier ones (either redirect them back to the correct step or make each step independently guard its own prerequisites).

On final submission, validate the complete aggregate model again (individual step validation is not a substitute for a final full-model check, since business rules can span steps), persist the result, and clear the in-progress state (session/draft record) so a stale wizard can't be resubmitted. Add `[ValidateAntiForgeryToken]` on every step's POST action.

Write tests covering: forward navigation persists data correctly, back navigation preserves it, direct-URL step-skipping is handled, and final submission validates the complete model.
