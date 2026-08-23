# Create a Reusable Component with Typed Parameters

**Category:** Blazor
**Use when:** Extracting duplicated markup/logic into a new reusable UI piece, or building a new component from a spec.

## Prompt

Analyze the surrounding feature area first, then propose a plan for a new reusable Blazor component before writing any code — follow the analyze -> propose -> approve -> implement -> test -> review workflow and wait for my go-ahead on the plan before implementing.

Design the component with strongly typed `[Parameter]` properties (never `object` or loosely typed dictionaries unless genuinely generic), and mark any parameter that must be supplied with `[EditorRequired]`. Distinguish clearly between one-way data flowing in via `[Parameter]`, two-way binding pairs (`Value` + `ValueChanged` + optional `@bind-Value` support), and callbacks exposed as `EventCallback`/`EventCallback<T>` rather than raw `Action`/`Func` delegates, since only `EventCallback` correctly participates in Blazor's automatic UI update pipeline.

Expose extensibility points via `RenderFragment` and `RenderFragment<T>` (e.g. a `ChildContent`, a per-item template, a header/footer fragment) instead of hardcoding markup, matching how existing components in this codebase expose templates. If the component wraps a native HTML element, forward unmatched attributes with `[Parameter(CaptureUnmatchedValues = true)]` onto a `Dictionary<string, object>` and splat them with `@attributes` so callers can still pass `class`, `id`, `aria-*`, etc.

Consider render performance: implement `IEquatable`-friendly parameter comparisons where relevant, avoid unnecessary `StateHasChanged()` calls, and use `[Parameter]` immutability correctly (never mutate a parameter's value in place). If the component allocates unmanaged resources, subscribes to events, or holds a JS interop reference, implement `IDisposable`/`IAsyncDisposable`.

Add or update bUnit tests covering parameter binding, event callback invocation, and RenderFragment content rendering. Confirm whether this targets Blazor Server, WebAssembly, or both, and flag anything that would behave differently under prerendering.
