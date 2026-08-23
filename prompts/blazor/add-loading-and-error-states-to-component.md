# Add Loading, Error, and Empty States to an Async Data Component

**Category:** Blazor
**Use when:** A component shows a blank screen, a stale UI, or crashes while fetching async data.

## Prompt

Before implementing, read the component's current data-fetch path (likely in `OnInitializedAsync` or `OnParametersSetAsync`) and identify every state it currently fails to represent: initial loading, empty result set, fetch error, and re-fetch-in-progress (e.g. when a parameter changes and triggers a reload while old data is still showing). Propose the state model — typically an enum or a small discriminated set of fields (`isLoading`, `errorMessage`, `data`) — before implementing.

Introduce explicit state fields set at the start and end of every fetch: set loading true and clear any previous error before the fetch begins, wrap the actual call in try/catch, populate a user-facing error message (not the raw exception message/stack trace, which may leak internal details) on failure, and set loading false in a `finally` block so it clears regardless of outcome. Render three distinct branches in markup — loading (skeleton or spinner), error (message plus a retry action if applicable), and success — plus an explicit empty state when the fetch succeeds but returns zero items, since "success with zero items" and "still loading" look identical to users if not handled separately.

Guard against race conditions when parameters can change while a fetch is in-flight: if `OnParametersSetAsync` can re-trigger a fetch before the previous one completes, track a request token/`CancellationTokenSource` (cancel and dispose the previous one before starting a new fetch) so a slow earlier response cannot overwrite the result of a newer request. Call `StateHasChanged()` (or `InvokeAsync(StateHasChanged)` if the continuation could resume on a different context) after state transitions if the framework wouldn't already re-render automatically.

If this component is also expected to prerender, ensure the loading state renders sensibly server-side before interactivity is established, and that the actual fetch doesn't run twice (once during prerender, once after the circuit/WASM boot) in a way that causes duplicate side effects — check `RendererInfo.IsInteractive` or the existing prerendering pattern used elsewhere in this codebase. Add a retry action wired to re-invoke the same fetch method. Add bUnit tests covering: initial loading render, successful render with data, empty-result render, and error render with a mocked failing dependency, plus a test that a stale in-flight request doesn't clobber a newer one if that race is realistically reachable.
