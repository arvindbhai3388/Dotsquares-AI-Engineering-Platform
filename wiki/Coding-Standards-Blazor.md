# Blazor Standards

Standards for Blazor Server and Blazor WebAssembly (WASM) components. General C#/.NET rules live in [C# Coding Standards](Coding-Standards-CSharp.md); the [ASP.NET Core/MVC/Razor page](Coding-Standards-AspNetCore-MVC-Razor.md) covers server-rendered alternatives when a feature doesn't need Blazor's interactivity at all.

## Component design

- **Single responsibility per component.** A component that renders a table, handles its own filtering, calls a service, and manages a modal is four components pretending to be one — split them.
- Favor small, composable components over large "page" components with hundreds of lines of markup. A page component should mostly orchestrate child components and hold page-level state, not contain deeply nested markup itself.
- Use `[Parameter]` for all data flowing into a component from its parent; never have a child component reach into global/ambient state to get data its parent already has, purely to avoid passing a parameter — this makes the component impossible to reuse or test in isolation.
- Use `EventCallback<T>` for child-to-parent communication, not a mutable shared object the child mutates and expects the parent to notice. Two-way binding (`@bind-Value`) is fine for simple form-field-style components; for anything more complex, prefer explicit `EventCallback` up and `[Parameter]` down.
- `CascadingValue`/`CascadingParameter` is for truly cross-cutting concerns (current theme, current user, a validation `EditContext`) shared by a whole subtree — not a substitute for passing parameters down a shallow tree.
- Prefer `RenderFragment`/`RenderFragment<T>` for content injection (a card component that renders arbitrary header/body/footer content) over building configuration-flag-driven components that try to cover every visual variant with parameters.
- Implement `IDisposable`/`IAsyncDisposable` on any component that subscribes to an event, a `PeriodicTimer`, a SignalR connection it owns, or a `CancellationTokenSource` it creates — Blazor does not do this for you, and undisposed subscriptions in Server components are a common source of memory leaks and continued work executing against a disconnected circuit.

## State management

- **Component-local state** (`private` fields) for anything scoped purely to that component's own UI (a "is dropdown open" flag). Do not lift state to a shared service unless another component genuinely needs to observe it.
- **Cascading state** (`CascadingValue`, or a scoped state-container service registered as `Scoped`) for state shared by a feature's component subtree — e.g., a multi-step wizard's current step and accumulated answers.
- **App-wide state** (a singleton or scoped service depending on Server/WASM, injected via DI, exposing an event other components subscribe to) for state genuinely global to the session, such as "current user's cart." Keep this to a small number of well-known state containers, not one ad hoc service per feature.
- In Blazor **Server**, be deliberate about what lives in a scoped service versus what's recomputed from the source of truth (usually a database) on each request — a scoped service in Server Blazor lives for the lifetime of the user's circuit (their browser tab/session), so state stored there is not automatically consistent with what another tab or another user sees, and does not survive a circuit reconnect the way it might survive a page reload in a traditional web app.
- In Blazor **WASM**, remember there is no server-side session at all by default — state lives in the browser's memory (or `localStorage`/`sessionStorage` via JS interop, or `ProtectedBrowserStorage`) and is lost on a hard refresh unless explicitly persisted.
- Avoid static fields/mutable static state for anything user-specific in either hosting model — in Server this leaks across circuits (and therefore across users) since a static field is shared process-wide, not per-circuit.

## Blazor Server vs. WebAssembly — decision criteria

| Consideration | Favors Server | Favors WebAssembly |
|---|---|---|
| Network reliability required by users | Poor/intermittent connectivity is a problem (every interaction is a SignalR round trip) | Works acceptably offline-first / on unreliable connections once the app is downloaded |
| Initial load time | Fast — no large WASM payload to download | Slower first load (runtime + app assemblies), improved by AOT/trimming but still heavier than Server |
| Server resource cost per user | Higher — each connected client holds server memory/CPU for its circuit; scales with concurrent users, not requests | Lower — most compute happens in the user's browser |
| Sensitive logic/data | Safe to keep server-side logic and secrets out of the client entirely | Must never ship secrets or sensitive business logic to the client — WASM code is fully inspectable by the user |
| Real-time collaborative features | Natural fit — already has a persistent SignalR connection | Requires its own SignalR client connection, same as any SPA |
| Offline capability | Not possible — requires a live connection to the server at all times | Possible with additional work (PWA support, service workers) |
| Client hardware/browser constraints | Works on low-powered devices since rendering logic runs server-side | Requires a modern browser with adequate WASM support and enough client CPU/memory |
| Latency sensitivity of interactions | Every UI event round-trips to the server — noticeable on high-latency networks | Interactions after load are local — no round trip for pure client-side logic |

Default recommendation for a typical Dotsquares internal line-of-business app on a corporate network: **Blazor Server** — lower client requirements, simpler secret handling, and the network-latency downside is usually immaterial on an internal network. Default for a public-facing or externally hosted app where server scaling cost per concurrent user matters, or where offline/PWA behavior is a requirement: **Blazor WebAssembly**. When genuinely uncertain, raise it explicitly in the [Propose](AI-Workflow-Discipline.md) step rather than defaulting silently — this is an architectural decision, not an implementation detail.

## JS interop rules

- Treat `IJSRuntime` calls as an external dependency, isolated behind a thin C# wrapper service (`IChartJsInterop`), not scattered `JSRuntime.InvokeVoidAsync("someGlobalFunction")` calls directly inside component markup/code-behind — this keeps components testable and makes the actual JS surface area auditable in one place.
- Prefer JS **isolation** (`import` of an ES module via `IJSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/MyLib/interop.js")`) over global `<script>` functions — it avoids global namespace collisions and loads only when needed.
- Always dispose `IJSObjectReference` module references in the component's `DisposeAsync`.
- Never pass secrets or server-only configuration values into JS interop calls — anything crossing into JS is visible in the browser's dev tools, which matters even in Blazor Server where the C# code itself is not exposed but the interop payloads are.
- In Blazor WASM specifically, JS interop calls are synchronous-capable in some scenarios (`IJSInProcessRuntime`) — prefer the async `IJSRuntime` API by default for consistency with Server, and only use the synchronous API where a measured performance need justifies the reduced portability.
- Validate/sanitize any data received back from JS before using it in a security-sensitive decision (e.g., don't trust a JS-reported "user is authenticated" flag) — JS interop is not a trust boundary crossing you can rely on for security decisions in WASM, since the client fully controls that code.

## Related pages

- [C# Coding Standards](Coding-Standards-CSharp.md)
- [SignalR Guidelines](SignalR-Guidelines.md) — the transport Blazor Server depends on.
- [Architecture Overview](Architecture-Overview.md)
