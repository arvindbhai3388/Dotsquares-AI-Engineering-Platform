# C# / .NET Coding Standards

General-purpose C#/.NET conventions that apply across every supported stack in this platform. Stack-specific standards live in their own pages: [ASP.NET Core/MVC/Razor](Coding-Standards-AspNetCore-MVC-Razor.md), [Blazor](Coding-Standards-Blazor.md), [Umbraco](Umbraco-Guidelines.md), [EF Core](EFCore-Guidelines.md).

## Naming

- **PascalCase**: types, methods, properties, public/protected fields, constants, namespaces.
- **camelCase**: local variables, method parameters, private fields (with a leading underscore: `_orderService`).
- **Interfaces** prefixed with `I` (`IOrderRepository`), never suffixed.
- **Async methods** suffixed with `Async` (`GetOrderAsync`), no exceptions for "it's obvious from context."
- Avoid abbreviations except universally understood ones (`Id`, `Url`, `Http`). `CustNum` is not acceptable; `CustomerNumber` is.
- Boolean properties/methods read as a predicate: `IsActive`, `HasPermission`, `CanExecute` — never `Status` for a boolean (make it an enum if it isn't binary).
- Generic type parameters: `T` alone for a single obvious parameter, otherwise a descriptive name with a `T` prefix (`TEntity`, `TResult`).

## Nullable reference types

- **NRT must be enabled** (`<Nullable>enable</Nullable>`) on every new project, and on existing projects being actively modified where the team has already opted in — do not disable it to silence warnings faster.
- A method parameter or return type not annotated `?` is a contract: the caller may assume non-null, and the implementation must guarantee it (throw rather than silently accept null if that contract is violated at a boundary such as a public API).
- Do not scatter `!` (null-forgiving operator) to make warnings go away. Each use should be justified in a comment when the reason isn't obvious from three lines of surrounding context (e.g., "validated non-null by ModelState above").
- At system boundaries (deserialized JSON, EF entities mapped from nullable DB columns, external API responses), treat everything as nullable until validated — this is where most real NREs originate, not in ordinary in-process method calls.
- Prefer `ArgumentNullException.ThrowIfNull(x)` (C# 10+) over manual `if (x is null) throw` boilerplate for parameter guards.

## Async/await

- **Async all the way down.** Never call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on a `Task` from otherwise-async-capable code — this is the single most common cause of deadlocks in ASP.NET and UI-thread contexts.
- Use `ConfigureAwait(false)` in library/class-library code that has no need to resume on a captured context (most application/service/data-access layers). It is not required in ASP.NET Core request-handling code, which has no synchronization context to deadlock on, but is harmless there too.
- Do not mark a method `async` if it contains no `await` — return the `Task` directly, or make it synchronous.
- Never use `async void` except for top-level event handlers (e.g., WinForms/WPF event handlers) where the signature is fixed by the framework. Everywhere else, `async void` swallows exceptions in a way that crashes the process or silently disappears them, rather than propagating to the caller.
- Pass and honor `CancellationToken` on any async method that does I/O and can reasonably be cancelled (HTTP calls, EF Core queries, long-running background work). Accept it as the last parameter with a default of `default` only at the outermost boundary where there's genuinely nothing to cancel yet.
- Prefer `Task.WhenAll` for independent concurrent operations over sequential `await`s in a loop, but be deliberate about it — concurrent DB calls sharing one `DbContext` are a bug, not a performance win (see [EF Core Guidelines](EFCore-Guidelines.md)).

## Dependency injection lifetimes

- **Singleton**: stateless services, configuration wrappers (`IOptions<T>`), caches, `HttpClient` factories. Never inject a scoped or transient service into a singleton's constructor without resolving it via `IServiceScopeFactory` at the point of use — a captive dependency (a shorter-lived service pinned to a singleton's lifetime) is a common source of stale-`DbContext` or stale-configuration bugs.
- **Scoped**: anything tied to a single request/unit of work — most importantly `DbContext`. One `DbContext` instance per request; never share one across concurrent operations or cache it in a field with a longer lifetime.
- **Transient**: lightweight, stateless services with no meaningful cost to constructing repeatedly. Default choice when a service holds no state and has no expensive setup.
- Register the narrowest lifetime that is correct — do not default everything to singleton "for performance"; the far more common bug in practice is a scoped-lifetime dependency accidentally captured as singleton, not the reverse.
- Constructor injection only. Avoid service locator patterns (`IServiceProvider.GetService<T>()` sprinkled through business logic) except in the specific, narrow case of resolving a scoped dependency from within a singleton via `IServiceScopeFactory`.

## Exception handling policy

- Exceptions signal **exceptional** conditions, not expected control flow. Use return types (`bool TryX(...)`, a result/outcome type, or nullable) for expected failure paths like "not found" or "validation failed" — reserve thrown exceptions for genuinely unexpected states (a broken invariant, an unreachable external dependency).
- Never catch `Exception` (or worse, bare `catch {}`) and swallow it silently. If you must catch broadly at a boundary (a background job runner, a top-level middleware), log with full context and either rethrow, fail the operation visibly, or handle it with a documented, deliberate fallback — never a silent no-op.
- Catch the most specific exception type you can meaningfully act on. Catching `SqlException` to detect a specific error number is fine; catching `Exception` to "be safe" around a two-line block is not.
- Do not use exceptions for validation of user input in normal request paths — return a `400`/`ProblemDetails` (see [ASP.NET Core/MVC/Razor standards](Coding-Standards-AspNetCore-MVC-Razor.md)) instead of throwing and catching a `ValidationException` per request.
- Preserve the original exception as `InnerException` when wrapping/rethrowing: `throw new ServiceException("...", ex);`, never `throw ex;` (which resets the stack trace) — use bare `throw;` to rethrow the current exception unchanged.
- Never log or rethrow an exception containing secrets, connection strings, or tokens in its message — see [Security Guidelines](../docs/Security-Guidelines.md).

## General

- Favor composition over inheritance; keep class hierarchies shallow.
- One class per file, filename matches the type name (standard .NET convention, and required for larger teams to navigate a solution by filename alone).
- Keep methods short enough to read without scrolling — if a method needs a comment marking "section 2 of 3," it should probably be three methods.
- Do not introduce a new third-party dependency for something the BCL already does well (see the platform's dependency guidance in the root [`CLAUDE.md`](../.claude/CLAUDE.md) §5).

## Related pages

- [ASP.NET Core, MVC & Razor Pages Standards](Coding-Standards-AspNetCore-MVC-Razor.md)
- [Blazor Standards](Coding-Standards-Blazor.md)
- [EF Core Guidelines](EFCore-Guidelines.md)
