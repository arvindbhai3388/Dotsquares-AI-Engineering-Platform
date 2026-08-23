# Add Request/Response Logging Middleware With Redaction

**Category:** ASP.NET Core
**Use when:** diagnosing production issues needs more request context without leaking secrets.

## Prompt

Analyze the current logging setup: which logging framework/sink is in use (built-in `ILogger`, Serilog, etc.), what's already logged per request (e.g., via existing middleware, `ILogger` calls in controllers, or a request logging library already referenced), and what headers, query parameters, and body fields this specific API surface routinely carries that count as sensitive — authentication tokens, API keys, passwords, PII, connection strings, or business-sensitive data specific to this domain. Enumerate the exact sensitive field/header names before designing redaction, don't guess a generic list.

Propose the design before implementing: what gets logged (method, path, status code, duration, correlation/trace ID always; headers and body only when actually useful for diagnostics and only after redaction), the redaction mechanism (an explicit denylist of header names like `Authorization`, `Cookie`, `X-Api-Key`, and body field names/JSON paths, replaced with a fixed `<REDACTED>` marker rather than omitted, so the log shape stays parseable), a cap on logged body size to avoid flooding logs with large payloads, and the log level (avoid Information-level logging of full bodies in production; consider Debug/Verbose gated by configuration).

Once approved, implement:
- Add middleware that wraps the request/response streams to capture body content only when logging is enabled for that level, buffering safely so downstream model binding still works (`EnableBuffering()` on the request body).
- Apply redaction consistently to headers and body fields before they're written to any log sink.
- Include a correlation ID (reuse an existing one if the app already generates one, e.g., `TraceIdentifier`) so a single request's log entries can be correlated.
- Make sure the middleware can't throw and break the actual request pipeline — wrap logging failures defensively.
- Never log full request/response bodies for endpoints handling authentication, payment, or credential data, even with redaction, unless explicitly required and approved.

Write or update tests confirming sensitive headers/fields are redacted in the logged output and that normal request handling behavior is unaffected by the buffering. Confirm with me before enabling body logging in any environment above Development/Staging.
