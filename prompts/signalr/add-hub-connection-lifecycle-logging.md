# Add Hub Connection Lifecycle Logging

**Category:** SignalR
**Use when:** it's hard to tell why or when clients are disconnecting in production.

## Prompt

Add structured logging for the connection lifecycle of the specified Hub so production disconnects, connects, and hub-level exceptions are diagnosable from logs instead of guessed at. First check the existing logging setup in the codebase (ILogger<T> usage, structured logging provider, correlation ID conventions) so this follows the same pattern rather than introducing a new logging style.

Implement:
- Override OnConnectedAsync to log connection establishment at Information level, including Context.ConnectionId, the resolved user identity (Context.UserIdentifier or relevant claim, never raw tokens or secrets), and any relevant request metadata (e.g., Context.GetHttpContext()?.Request.Headers["User-Agent"], transport type) -- then call base.OnConnectedAsync().
- Override OnDisconnectedAsync(Exception? exception) to log disconnection at Information level when exception is null (graceful disconnect) and at Warning or Error level when exception is not null, including the exception details and ConnectionId -- this is the primary signal for diagnosing "why did the client drop" in production, since a null exception means clean close/transport timeout while a populated one means an actual fault.
- Add an IHubFilter (or per-method try/catch if a filter is out of scope for this task) that logs unhandled exceptions thrown from hub method invocations before they're translated to the client, capturing method name, ConnectionId, and correlation ID if the app has one, without logging full request payloads if they may contain sensitive data.
- Include enough context to correlate a disconnect with the corresponding connect (e.g., log connection duration on disconnect by tracking connect time, or ensure ConnectionId alone is sufficient to join the two log lines in the log aggregation tool already in use).
- Never log secrets, tokens, connection strings, or full user PII -- redact/omit sensitive claims and only log identifiers needed for correlation.
- Keep log volume reasonable: avoid Debug/Trace-level noise on every heartbeat; focus on connect, disconnect (with reason), and exception events.

After implementing, verify by running the app locally (or via existing integration tests) and confirming a normal disconnect, a network-drop disconnect, and a method-level exception all produce distinguishable log entries. Report which scenarios were actually exercised versus which require a production-like environment to confirm.
