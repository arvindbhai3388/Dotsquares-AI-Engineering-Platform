# Add Virtualize for a Large Data List

**Category:** Blazor
**Use when:** A list or grid renders hundreds or thousands of items eagerly and the UI becomes sluggish to scroll or render.

## Prompt

Before implementing, measure or estimate the actual item count and item render cost (how much markup/logic per row) and confirm whether the data source can support paged/windowed retrieval (an `ItemsProvider` calling a paged API) versus being fully materialized in memory already (`Items="@allItems"` mode) — propose which `Virtualize` mode fits before implementing, since retrofitting from in-memory to provider-based virtualization later is a larger change.

Replace the eager `@foreach` over the full collection with `<Virtualize Context="item" ItemsProvider="LoadItems" ItemSize="...">` (provider mode) or `<Virtualize Context="item" Items="@allItems">` (fixed-collection mode), keeping the same per-item template markup so visual output is unchanged. Set `ItemSize` to a realistic average pixel height of a rendered row if rows are a consistent height — an inaccurate `ItemSize` degrades scroll estimation and causes visible jumping. If row heights vary significantly, be explicit that `Virtualize` assumes roughly uniform item size and flag this as a known limitation rather than silently shipping janky scrolling.

For provider mode, implement the `ItemsProviderDelegate<T>` to accept the `ItemsProviderRequest` (`StartIndex`, `Count`, `CancellationToken`), call the paged data source with those bounds, and return a `ItemsProviderResult<T>` with the page plus the total count; honor the provided `CancellationToken` so a fast-scrolling user doesn't pile up stale in-flight requests. Ensure the containing element has a bounded, scrollable height (`overflow-y: auto` with a fixed or flex-computed height) — `Virtualize` only virtualizes within a scrollable viewport, and without one it will still try to render everything.

Confirm this list isn't also driving `OnAfterRender` side effects per item (e.g. per-row JS interop) that would now behave differently since off-screen items no longer render — audit for lifecycle assumptions tied to "every item is always in the DOM." Test manually with a realistic (or synthetic large) dataset to confirm smooth scrolling and correct placeholder rendering, and add a bUnit test verifying the items provider is called with expected `StartIndex`/`Count` ranges as the visible window changes, plus a test for empty and single-page datasets.
