# Convert a Synchronous Action to Async End-to-End

**Category:** ASP.NET Core
**Use when:** a blocking I/O call is found in a hot request path.

## Prompt

Locate the controller action or endpoint handler I specify and trace its full call chain: every method it calls directly or transitively, down to the actual I/O boundary (database call, HTTP client call, file I/O, or `Task.Result`/`.Wait()`/`.GetAwaiter().GetResult()` usage). Identify every synchronous-over-asynchronous or blocking call in that chain, not just the one I initially pointed at.

Before changing anything, propose the conversion plan: which method signatures change from `T` to `Task<T>` (or `ValueTask<T>` where appropriate), which now need a `CancellationToken` parameter threaded from the incoming `HttpContext.RequestAborted`, which interfaces/abstractions need their contracts updated (and therefore which other implementations or mocks in the codebase will need matching changes), and whether any of this touches a `DbContext` or shared service that isn't safe to call concurrently — call out anything that looks like it fans out into a large blast radius before touching it.

After I approve, implement the conversion:
- Change signatures to `async Task<IActionResult>`/`async Task<T>` consistently down the chain — no partial conversions that leave a blocking call in the middle.
- Thread the `CancellationToken` through to every awaited call that accepts one (DB queries, `HttpClient` calls, file streams).
- Replace `.Result`/`.Wait()`/`.GetAwaiter().GetResult()` with proper `await`.
- Avoid `async void`; avoid unnecessary `Task.Run` wrapping of already-async work.
- Check for and fix any now-invalid `lock` statements around code that now awaits inside the lock.

Write or update tests to cover successful completion, and cancellation via a pre-cancelled `CancellationToken` producing the expected behavior (typically an `OperationCanceledException` surfaced correctly, not swallowed). Run the affected project's build and test suite and report the actual results, not an assumption that it compiles.
