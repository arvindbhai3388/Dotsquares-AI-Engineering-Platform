# Add a Custom Tag Helper for a Recurring Markup Pattern

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a pattern (e.g., a formatted currency span, a permission-gated block) recurs across views with inline logic.

## Prompt

There's a UI pattern (for example: a formatted currency/date span, a permission-gated block, a status badge with conditional CSS classes) that currently gets reimplemented inline with `@if`/string formatting/helper method calls scattered across several Razor views. Find each occurrence and confirm the underlying logic is genuinely the same rule everywhere, not multiple similar-looking rules that happen to render similarly.

Propose a custom tag helper as the fix: its element/attribute name (following this project's naming conventions if any tag helpers already exist, otherwise a clear, non-colliding name), its bound properties (`[HtmlAttributeName]`), and whether it should render an element itself or just apply attributes to the existing tag (`Process`/`ProcessAsync` modifying `TagHelperOutput` vs. suppressing the output tag with `output.TagName = null`). Show me the API surface before implementing so call sites are predictable.

Implement the tag helper in the project's existing tag-helpers location (or create one following the existing folder-per-concern convention), register it via `_ViewImports.cshtml` `@addTagHelper`, and ensure it fails safely on null/missing input (render nothing or a sensible default rather than throwing) since Razor tag helpers execute per-request and an unhandled exception there breaks the whole page. Encode any user-supplied text it renders; do not bypass encoding with raw HTML output unless the content is already known-safe and that's documented.

Replace the inline duplicated logic at each call site with the new tag helper, verifying rendered output is unchanged. Write unit tests directly against the tag helper class (constructing it, setting bound properties, calling `Process`, asserting on `TagHelperOutput`) covering the normal case, null/missing input, and any permission/edge-case branch, before calling this done.
