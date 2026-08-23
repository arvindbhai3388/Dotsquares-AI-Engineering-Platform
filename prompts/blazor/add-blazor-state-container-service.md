# Add a Scoped State Container Service

**Category:** Blazor
**Use when:** Sibling or distant components need to share and react to changing state, and parameter drilling or static fields are not appropriate.

## Prompt

Before implementing, confirm the exact state being shared and its natural lifetime scope — per-user-session state in Blazor Server should be registered `Scoped` (one instance per circuit), never `Singleton` (which would leak one user's state to every other user) and never a static field (same problem, plus thread-safety issues across circuits in the same process). For Blazor WebAssembly, `Scoped` and `Singleton` behave identically since each browser tab is its own process, but keep the registration `Scoped` anyway for consistency and portability. Propose the service's public API (properties, mutation methods, change notification) before writing it.

Implement the container as a plain class exposing an event (`public event Action? OnChange;` or `event Action<T>? OnChange` if consumers need the new value) that mutation methods invoke after updating internal state — never expose public settable properties directly, force state changes through named methods (`SetSelectedItem(item)`, `AddToCart(item)`) so the logic stays auditable and testable. Register it in DI as scoped in the appropriate `Program.cs`/`Startup` composition root.

In consuming components, inject the service with `[Inject]`, subscribe to `OnChange` in `OnInitialized` by wiring it to call `InvokeAsync(StateHasChanged)` (required in Blazor Server because the change may originate from a different synchronization context than the one rendering the component), and unsubscribe in `Dispose()` by implementing `IDisposable` — a missed unsubscription here is a classic memory leak, since the container will keep a reference to a disposed component's handler for the lifetime of the circuit.

Avoid making the container itself perform I/O directly if that couples UI state to data access — consider having it call an injected repository/service instead, and keep the container's own logic synchronous and side-effect-free besides the change notification. Add unit tests (not necessarily bUnit — plain xUnit/MSTest against the container class matching this codebase's existing test framework) verifying mutation methods update state correctly and fire `OnChange` exactly once per logical change, plus a bUnit test confirming a consuming component re-renders when the container changes.
