# Add Global Exception-Handling Middleware

**Category:** ASP.NET Core
**Use when:** error responses are inconsistent or leak stack traces across an API.

## Prompt

Analyze the current error-handling state of this API: whether `UseExceptionHandler`, a custom `IExceptionHandler` (.NET 8+), or ad hoc try/catch blocks scattered across controllers/endpoints are currently used, what the existing error response shapes look like across a sample of endpoints, and whether the Development environment currently exposes `UseDeveloperExceptionPage` in a way that must be preserved for local debugging.

Propose the design before implementing: a single centralized handler (an `IExceptionHandler` implementation registered via `AddExceptionHandler` and `UseExceptionHandler`, or middleware if the project targets an older pattern) that maps exception types to HTTP status codes and a consistent RFC 7807 ProblemDetails body — e.g., validation exceptions to 400, not-found/domain "entity missing" exceptions to 404, authorization failures to 403, and everything else to a generic 500 with no internal details exposed. Confirm which existing custom exception types in the codebase need explicit mapping versus falling through to the generic handler, and how correlation/trace IDs (`Activity.Current?.Id` or `HttpContext.TraceIdentifier`) should be included in the response for support purposes.

Once approved, implement:
- Register the handler early in the middleware pipeline, ensuring it runs before any middleware that could itself throw unhandled.
- Log the full exception (type, message, stack trace) server-side via the existing logging framework, but only return a safe, redacted message and status code to the client — never the raw exception message or stack trace in production.
- Preserve any existing status codes/response shapes that clients already depend on, unless this task is explicitly meant to change them (call this out if so).
- Ensure the handler itself cannot throw (wrap logging in a way that can't cause a secondary unhandled exception).

Write or update tests that throw each mapped exception type from a test endpoint/handler and assert the correct status code and ProblemDetails shape, plus a test for an unmapped exception falling through to the generic 500 response. Confirm with me before changing the response shape of any endpoint already consumed by external clients.
