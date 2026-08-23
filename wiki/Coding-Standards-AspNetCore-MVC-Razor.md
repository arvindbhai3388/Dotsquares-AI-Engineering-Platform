# ASP.NET Core, MVC & Razor Pages Standards

Standards specific to ASP.NET Core Web API/minimal APIs, ASP.NET MVC (including legacy ASP.NET MVC 5 on .NET Framework, common on older client codebases), and Razor Pages. General C#/.NET rules (naming, NRT, async, DI, exceptions) live in [C# Coding Standards](Coding-Standards-CSharp.md) and apply here too.

## Thin controllers / thin page models

Controllers, minimal API endpoint delegates, and `PageModel`s are **orchestration only** — they:

1. Bind and validate the incoming request.
2. Call into an application/service-layer method (see [Architecture Overview](Architecture-Overview.md), layer 2).
3. Map the result to a response DTO/view model and return it.

They must never contain business logic, direct data access (no `DbContext` injected into a controller), or multi-step orchestration of several services with branching logic — that belongs in a service class that can be unit-tested without spinning up ASP.NET Core's hosting pipeline.

```csharp
// Good — thin controller delegating to a service
[HttpPost]
public async Task<ActionResult<OrderResponse>> CreateOrder(
    CreateOrderRequest request, CancellationToken ct)
{
    var result = await _orderService.CreateOrderAsync(request.ToCommand(), ct);
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetOrder), new { id = result.Value.Id }, result.Value)
        : Problem(statusCode: 422, detail: result.Error);
}
```

A controller action longer than ~15–20 lines, or one with nested `if`/`try` blocks implementing business rules, is a signal the logic belongs one layer down.

## View model / DTO separation

- **Never bind a request directly to an EF Core entity**, and never return an entity directly from an API response. Over-posting (a client supplying extra fields your entity happens to have, like `IsAdmin`, that get silently bound) and accidental exposure of internal/navigation properties are both real, recurring vulnerabilities this separation exists to prevent.
- Use dedicated request models (`CreateOrderRequest`), response models (`OrderResponse`), and, for server-rendered views, view models (`OrderDetailsViewModel`) distinct from both. Mapping between them is either manual (preferred for simple shapes, keeps intent explicit) or via a mapper (AutoMapper/Mapster) for larger, repetitive shapes — pick one approach per project and keep it consistent.
- Razor views/pages bind only to their view model, never to a domain entity or a raw `DbContext` query result.

## Model validation

- Use `DataAnnotations` for simple, declarative rules (`[Required]`, `[StringLength]`, `[Range]`) directly on request models; use FluentValidation for anything conditional, cross-field, or requiring async lookups (e.g., "email must be unique").
- Check `ModelState.IsValid` (MVC/Razor Pages) explicitly unless `[ApiController]` is applied, which triggers automatic `400` responses with a `ValidationProblemDetails` body for invalid models — prefer `[ApiController]` on Web API controllers so this is handled uniformly instead of hand-rolled per action.
- Validate at the boundary (request model), not deep inside a service — by the time a command reaches the service layer, its invariants should already hold, so the service layer trusts the shape but should still defensively validate business rules (e.g., "does this order total match its line items") that a request-model annotation cannot express.

## API versioning

- New Web APIs default to explicit versioning via `Asp.Versioning.Mvc` (the actively maintained fork of the retired `Microsoft.AspNetCore.Mvc.Versioning`), using URL segment versioning (`/api/v1/orders`) as the default convention unless a client's existing API already uses header or query-string versioning, in which case match the existing convention rather than introducing a second scheme.
- Never make a breaking change to a shipped API version — additive changes (new optional fields, new endpoints) do not require a version bump; removing/renaming a field, changing a status code's meaning, or changing required-ness does.
- Deprecate before removing: mark an old version `[ApiVersion("1.0", Deprecated = true)]` and communicate a sunset window before it's actually removed.

## `ProblemDetails` error responses

- All API error responses use RFC 7807 `ProblemDetails` (`Microsoft.AspNetCore.Mvc.ProblemDetails` / `ValidationProblemDetails`), not ad hoc `{ "error": "..." }` shapes. ASP.NET Core's built-in exception handling middleware and `[ApiController]` produce these automatically for unhandled exceptions and model validation failures respectively — extend that middleware rather than replacing it with custom error-shaping per controller.
- Populate `type`, `title`, `status`, and `detail` meaningfully; use `extensions` for machine-readable error codes a client needs to branch on, rather than encoding that information only in free-text `detail`.
- Never put exception messages, stack traces, or internal identifiers (connection strings, SQL fragments) into a `ProblemDetails` returned to the client in production — map internal exceptions to a safe, generic `detail` and log the real exception server-side. Development-only environments may enable `UseDeveloperExceptionPage` for local debugging, never in a deployed environment.

## Razor Pages specifics

- One `PageModel` per feature/page, colocated `.cshtml`/`.cshtml.cs`. Do not build a general-purpose "shared" `PageModel` base class that accumulates unrelated helper methods — favor small, composed services injected into each page model instead.
- Use `OnGetAsync`/`OnPostAsync` handler naming consistently; named handlers (`OnPostDeleteAsync` bound via `asp-page-handler="Delete"`) for pages with more than one form action.

## ASP.NET MVC (legacy) notes

For client codebases still on ASP.NET MVC 5 / .NET Framework (not ASP.NET Core MVC):

- The same thin-controller and view-model-separation rules apply unchanged — they predate ASP.NET Core.
- There is no built-in `ProblemDetails`; if the codebase doesn't already have an equivalent convention, discuss introducing one with the client team rather than inventing a new ad hoc error shape (see the root platform [`CLAUDE.md`](../.claude/CLAUDE.md) on matching each project's own conventions).
- Model binding is more permissive by default (no automatic `400` on invalid `ModelState`) — validation must be checked explicitly in every action.

## Related pages

- [C# Coding Standards](Coding-Standards-CSharp.md)
- [Blazor Standards](Coding-Standards-Blazor.md)
- [Architecture Overview](Architecture-Overview.md)
