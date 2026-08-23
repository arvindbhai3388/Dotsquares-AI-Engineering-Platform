# Create a Disposable JS Interop Wrapper

**Category:** Blazor
**Use when:** A component needs to call a JS library or browser API that Blazor does not expose natively.

## Prompt

Before writing code, confirm with me which JS API/library is being wrapped, whether it needs to call back into .NET (requiring a `[JSInvokable]` callback and a `DotNetObjectReference`), and whether this must work under Blazor Server (network round-trip per call, thread affinity per circuit) or WASM (synchronous-capable via `IJSInProcessRuntime` but still async-by-default) or both. Propose the wrapper's public C# surface as an interface before implementing.

Implement the wrapper as a class that takes `IJSRuntime` via constructor injection and lazily loads the JS module with `await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/{Assembly}/{module}.js")`, caching the `IJSObjectReference` in a `Lazy<Task<IJSObjectReference>>` or equivalent so the import only happens once per instance. Every public method should await the module task first, then call `module.InvokeVoidAsync`/`InvokeAsync<T>` with a `CancellationToken` parameter threaded through where the caller can supply one.

Implement `IAsyncDisposable` on the wrapper: dispose of the `IJSObjectReference` module reference and any `DotNetObjectReference<T>` created for callbacks, and guard against disposing twice. If the wrapper is registered as scoped/singleton DI service rather than owned by a single component, make sure its lifetime doesn't outlive the JS-side state it references (e.g. don't hold a singleton wrapper that references a `DotNetObjectReference` tied to a specific component instance).

Never call JS interop methods from `OnInitialized`/`OnParametersSet` — only from `OnAfterRenderAsync(firstRender)` or later, and guard every call site against prerendering (`IJSRuntime` calls during static server-side prerendering throw `InvalidOperationException` because there is no browser yet); either check `firstRender`, use a `_isPrerendering` flag cleared in `OnAfterRenderAsync`, or wrap calls in a try/catch that no-ops during prerendering with a comment explaining why. Write bUnit tests that inject a mocked `IJSRuntime` (via `TestContext.JSInterop.Setup...`) to verify the wrapper invokes the expected JS function names and arguments, and verify disposal actually calls the JS-side cleanup function.
