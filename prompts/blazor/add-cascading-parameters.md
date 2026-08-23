# Add CascadingValue/CascadingParameter for Shared Context

**Category:** Blazor
**Use when:** Multiple nested components need the same contextual data (theme, current user, tenant) without prop drilling through every intermediate level.

## Prompt

Before implementing, identify the actual scope of the data being shared (page-level, layout-level, or app-wide) and propose where the `<CascadingValue>` should be placed — usually as high as the layout or a wrapping component, but no higher than necessary, since an overly broad cascading value forces every descendant to re-render whenever it changes. Wait for my approval of the placement before implementing.

Define a dedicated, immutable-by-convention type for the cascaded value (e.g. a `ThemeContext`, `CurrentUserContext`, or `TenantContext` record/class) rather than cascading primitive types or loosely typed dictionaries — this keeps `[CascadingParameter]` consumers type-safe and self-documenting. If more than one cascading value of the same type could exist in the tree, use a `Name` on both `<CascadingValue Name="...">` and `[CascadingParameter(Name = "...")]` to disambiguate.

Decide explicitly whether the cascading value should be `Fixed` (`<CascadingValue Value="..." IsFixed="true">`). If the value never changes after initial render for a given component instance, mark it fixed — this is a meaningful performance optimization because Blazor skips subscribing descendants to change notifications for fixed cascading values. If the value does change at runtime (e.g. theme toggle, tenant switch), do not mark it fixed, and make sure the source component calls `StateHasChanged()` when the underlying value changes so cascading consumers actually re-render.

Update every consuming component to declare `[CascadingParameter]` with the correct type (and `Name` if used), and confirm none of them assume the value is available during `OnInitialized` if it could only be set after an async fetch — guard with null checks or expose a loading state. Add bUnit tests using `RenderTree.Add<CascadingValue<T>>` (or `TestContext.RenderComponent` with a cascading parameter override) to verify consumers pick up both the initial value and subsequent changes. Flag any place where cascading was used as a substitute for a proper DI-scoped state service — if the data needs imperative mutation methods, an injected state container (see the state-container prompt) is usually a better fit than cascading.
