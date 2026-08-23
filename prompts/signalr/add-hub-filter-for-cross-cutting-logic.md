# Add an IHubFilter for Cross-Cutting Logic

**Category:** SignalR
**Use when:** the same boilerplate (logging, validation, exception handling) is duplicated across every hub method.

## Prompt

Add an IHubFilter to the specified Hub(s) to centralize <describe the cross-cutting concern, e.g., logging every invocation, validating a common parameter shape, translating exceptions into HubException> instead of duplicating it in every method body. First identify every hub method that currently repeats this logic so the filter genuinely replaces all of it (and none of it needs to remain duplicated after the filter is added), then propose the filter's scope and registration before implementing.

Implementation requirements:
- Implement IHubFilter and override InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next) for per-method-call logic, and OnConnectedAsync/OnDisconnectedAsync overloads only if the concern applies to connection lifecycle rather than method invocation.
- For logging: capture context.HubMethodName, context.HubMethodArguments (redact/omit any sensitive arguments -- never log tokens, passwords, or PII), and context.Context.ConnectionId, timing the call around the `await next(context)` to log duration, and log exceptions from `next` before rethrowing (or translating) them.
- For validation: if the same argument-shape or authorization check is repeated across methods, note that IHubFilter can inspect context.HubMethodArguments generically, but object-level authorization (e.g., "can this user access resource X") is usually better handled in each method or a shared helper it calls, since the filter can't easily know the semantic meaning of each method's arguments without per-method configuration -- decide and document which checks belong in the filter versus in the method body, don't force everything into the filter just because it's centralized.
- For exception handling: catch expected exceptions inside the filter and translate them into a client-safe HubException; let truly unexpected exceptions propagate (after logging) so they aren't silently swallowed.
- Register the filter via services.AddSignalR(options => options.AddFilter<TFilter>()) for global application, or via [HubMethodName]-adjacent per-hub registration if it should only apply to specific hubs -- confirm which scope is intended before implementing globally.
- Ensure filters are ordered correctly if multiple filters are registered (they run in registration order) and that adding this filter doesn't change behavior for methods that didn't need the cross-cutting logic (e.g., don't introduce new validation failures for methods that were fine before).

After approval, implement the filter, remove the now-redundant duplicated code from each hub method, and add a test that verifies the filter actually runs for a sample method (e.g., asserting the expected log entry or exception translation), plus confirm existing hub method tests still pass unmodified in behavior.
