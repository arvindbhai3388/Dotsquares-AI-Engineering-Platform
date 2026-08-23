# Map Entities to a View Model Cleanly

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a controller is currently passing entities directly to views.

## Prompt

Locate the controller action(s) I point you to that currently pass an EF/domain entity (or a list of entities) straight to a Razor view. This is a maintainability and over-posting risk: the view can end up bound to internal fields, and POST-back model binding can overwrite properties that were never meant to be user-editable.

First, understand the current flow: what the entity contains, which of those fields the view actually renders or edits, and whether any computed/derived display values are currently done inline in Razor (`@if`, string formatting, null-coalescing) that belong in the view model instead. Propose a view model shape before writing code and get my approval.

Then decide, and justify the choice, between manual mapping (a static `ToViewModel()` extension method or mapping method colocated with the view model, or in a small mapper class if the project already has one) versus introducing AutoMapper. Do not add AutoMapper as a new dependency unless the codebase already uses it elsewhere or the mapping is complex enough (many nested objects, conditional logic) that manual mapping would be unreasonably verbose -- flag this as a dependency decision per project conventions rather than adding it silently.

Update the controller to map entity -> view model on the way out, and view model -> entity (only for the fields that should be editable) on the way in for POST actions, being explicit about which properties are intentionally excluded from the write-back to prevent over-posting. Preserve existing validation attributes and error messages. Update the Razor view's model type accordingly and fix any `@model` references or `Html.DisplayFor`/`Html.EditorFor` calls that assumed entity properties.

Write or update unit tests verifying the mapping is correct in both directions, including edge cases like null navigation properties, before considering this done. Report exactly which properties were intentionally excluded from write-back and why.
