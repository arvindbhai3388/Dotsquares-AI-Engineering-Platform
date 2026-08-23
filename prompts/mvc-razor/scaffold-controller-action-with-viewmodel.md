# Scaffold a Controller Action Backed by a View Model

**Category:** ASP.NET MVC / Razor Pages
**Use when:** adding a new MVC page/flow from scratch.

## Prompt

I need a new MVC page/flow added to this codebase. Before writing any code, run the Understand -> Locate -> Plan -> Approve -> Implement -> Test -> Review workflow: identify the target controller (or propose a new one following existing naming/folder conventions), the route, and the HTTP verb(s) involved, and show me the plan before implementing.

When you implement, do not pass the raw EF/domain entity to the view. Create a dedicated view model class (in the project's existing ViewModels location and namespace convention) that exposes only the fields the view actually needs, with appropriate data annotations (`[Required]`, `[StringLength]`, `[Display(Name = ...)]`, `[DataType]`) matching validation rules already enforced elsewhere in the domain. Add a GET action that builds and returns the view model, and a POST action that accepts the view model, checks `ModelState.IsValid` before touching persistence, and redirects (PRG pattern) on success rather than returning a view directly from POST.

Apply `[ValidateAntiForgeryToken]` on the POST action and confirm the Razor view emits `@Html.AntiForgeryToken()` (or the tag-helper equivalent) inside the form. Handle the null/not-found case explicitly (e.g., return `NotFound()`/`HttpNotFoundResult` rather than letting a null reference throw). Encode any user-supplied values rendered back into the view to avoid XSS; do not use `Html.Raw` on untrusted input.

Write the failing unit test(s) first for the new action(s) in the project's paired test project, covering: valid submission, invalid ModelState, not-found/authorization failure, and redirect-on-success, before writing the implementation. After implementing, run the tests, confirm they pass, and summarize what changed, what was tested, and any follow-up needed (e.g., missing authorization checks) in the review step.
