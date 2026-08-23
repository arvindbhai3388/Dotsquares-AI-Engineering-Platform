# Convert ViewBag/ViewData to a Strongly Typed View Model

**Category:** ASP.NET MVC / Razor Pages
**Use when:** a view relies on loosely typed ViewBag properties that are error-prone and untestable.

## Prompt

The view (and its controller action(s)) I point you to relies on `ViewBag`/`ViewData` for data the view needs to render. This is error-prone (typos in the dynamic property name fail silently at runtime, not compile time) and makes the action untestable without inspecting the dynamic bag. Convert it to a strongly typed view model.

First, read the view and every controller action that renders it (including any partial views it includes) and enumerate every distinct `ViewBag.X`/`ViewData["X"]` key actually used, along with its real type (inferred from how it's assigned and how it's consumed in Razor -- watch for a key used inconsistently as different types across different actions, which is itself a bug worth flagging). Show me the full list before designing the view model so nothing gets missed.

Design a view model class containing a strongly typed property for each key, with a sensible type (not `object` or `dynamic`) and nullability that matches whether the controller always sets it. Update every controller action that renders this view to populate the view model instead of `ViewBag`/`ViewData`, and update the view's `@model` declaration and every reference from `@ViewBag.X`/`@ViewData["X"]` to `@Model.X`, including inside any nested partials that were also reading from the bag (partials share the parent's ViewData by default, so check whether removing ViewBag usage breaks a partial that depended on it implicitly).

Do not leave a mix of ViewBag and view model for the same view -- convert every usage found in the audit, or explicitly justify to me why one specific value should stay dynamic (rare, but possible for truly cross-cutting layout data like a page title set by `_ViewStart`). Verify rendered output is unchanged, and add/update unit tests asserting the controller action populates the view model correctly for each previously dynamic value.
