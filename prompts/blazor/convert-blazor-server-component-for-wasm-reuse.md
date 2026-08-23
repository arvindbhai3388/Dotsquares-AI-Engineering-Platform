# Convert a Blazor Server Component for WebAssembly Reuse

**Category:** Blazor
**Use when:** A component built for Blazor Server needs to also run in a WebAssembly or hybrid-hosted app.

## Prompt

Before changing anything, locate every server-only dependency this component relies on and report them back to me as a plan: direct database/EF/HttpContext access, server-side DI services registered only in the Server project's `Program.cs`, use of `IWebHostEnvironment`, synchronous blocking I/O assumed safe because it runs on the server, or reliance on `HttpClient` instances pointed at relative URLs (which only work server-side because of `BaseAddressAuthorizationMessageHandler`/server routing). Do not implement until I approve the plan.

For each server-only dependency, extract an interface and either (a) provide a WASM-side implementation that calls a Web API instead of touching data directly, or (b) push the dependency up to the host app via constructor injection so the component itself stays render-mode agnostic. If the component uses `IJSRuntime`, verify calls are wrapped in checks or deferred past `OnAfterRenderAsync(firstRender)`, since WASM's JS interop timing and Server's SignalR-circuit-based interop timing differ, and un-guarded interop calls during prerendering will throw in both models.

Replace any use of `HttpContext`/cookie-based auth reads with a WASM-compatible auth pattern (e.g. `AuthenticationStateProvider`) if authentication state is needed. Check for thread-affinity assumptions: Blazor Server code sometimes assumes a single synchronization context per circuit and takes shortcuts around locking that are not guaranteed to hold in a WASM single-threaded UI thread either, so do not introduce new assumptions — keep state access consistent with `InvokeAsync(StateHasChanged)` patterns already used in this codebase.

If this is meant to work in both hosting models simultaneously (not just WASM), structure the component as a Razor Class Library target so it is compiled against `netstandard2.1`/`net8.0` without a hard reference to `Microsoft.AspNetCore.Components.Server`. Add or update bUnit tests that run the component logic in isolation from any specific render mode, and validate manually in both hosting models before calling this done.
