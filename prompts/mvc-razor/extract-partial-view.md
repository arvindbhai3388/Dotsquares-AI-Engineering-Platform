# Extract Repeated Razor Markup into a Partial View

**Category:** ASP.NET MVC / Razor Pages
**Use when:** the same markup block is copy-pasted across multiple views.

## Prompt

I've identified (or want you to find) a block of Razor markup that's duplicated across multiple views in this project. Locate every occurrence -- do not assume there are only two; search the relevant Views folder(s) for the repeated structure or a distinctive CSS class/id inside it -- and confirm they are actually the same markup with the same intent, not superficially similar blocks that happen to look alike but serve different purposes (don't force-merge those).

Once confirmed, propose the shape of a partial view: its name (following this project's existing partial naming convention, typically prefixed with `_`), its expected model type (a shared view model, a subset of an existing one, or a small dedicated model -- pick the smallest one that covers every call site), and which call sites will use `@await Html.PartialAsync(...)`/`<partial name="..." model="..." />` versus needing `RenderPartialAsync` for streaming-sensitive contexts. Get my approval on the shape before extracting.

When extracting, preserve exact rendered output (same HTML, same CSS classes, same encoding behavior) so this is a pure refactor with no visual or behavioral change -- do not "improve" markup while extracting it unless I ask. Make sure any inline `@if`/`@foreach` logic in the original blocks that differs slightly between call sites is either reconciled into shared logic or exposed as a model property/flag on the partial's view model, not left as copy-pasted variations inside the partial.

After extraction, verify each call site renders identically by comparing before/after HTML output where feasible, and check that any JavaScript or CSS scoped to the original markup (by id or class) still targets it correctly, especially if the partial can now render more than once per page and needs unique ids. Report which files were changed and confirm no view lost functionality.
