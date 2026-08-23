# Handle Blazor Server Circuit Disconnect/Reconnect Gracefully

**Category:** Blazor
**Use when:** Users on flaky connections lose their session, see a frozen UI, or lose unsaved work when the SignalR circuit drops.

## Prompt

Before implementing, clarify this is specific to Blazor Server's circuit model: the app's interactive state lives server-side tied to a SignalR connection, so any network blip drops that connection and Blazor's default reconnection UI (`components-reconnect-modal`) takes over, but the default styling/behavior is often not acceptable for production. Propose the reconnection UX (custom modal content, retry/backoff behavior, what happens if reconnection ultimately fails) before implementing.

Customize the reconnection UI by overriding the default `#components-reconnect-modal` CSS (hidden/visible states keyed off `components-reconnect-hide`/`-show`/`-failed`/`-rejected` classes that the Blazor Server JS toggles automatically) in the host page, rather than fighting the built-in mechanism. If retry timing needs tuning, configure `CircuitOptions`/the reconnection interval array in the `Blazor.start()` call in the host page's script rather than reinventing reconnection logic in C#.

For state recovery: identify what in-progress work (form input, wizard step, unsaved edits) would be lost on a circuit rebuild, since a new circuit means a completely fresh component tree and lost in-memory state — server-side state containers, cascading values, and injected scoped services are all recreated from scratch. For anything that must survive a reconnect, persist it client-side (browser `localStorage`/`sessionStorage` via JS interop, keyed by a stable identifier) and rehydrate it in `OnAfterRenderAsync(firstRender)` after the new circuit establishes, or persist server-side keyed by a durable session/user identifier rather than the ephemeral circuit ID.

Add explicit handling for the "reconnection failed/rejected" terminal states (the modal's failed/rejected classes) — surface a clear message directing the user to refresh, since Blazor Server will not retry indefinitely. If this app has long-running operations that could be mid-flight during a disconnect (file uploads, multi-step forms), make sure those operations are idempotent or resumable rather than silently lost. Validate manually by simulating a disconnect (throttle/kill network in dev tools) and confirming the reconnect UI appears, reconnects when the network returns, and any persisted state rehydrates correctly; there is no meaningful bUnit coverage for circuit-level behavior, so call out that this needs manual/integration verification instead.
