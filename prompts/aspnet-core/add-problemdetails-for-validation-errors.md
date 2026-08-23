# Standardize Validation Errors Using ProblemDetails

**Category:** ASP.NET Core
**Use when:** 400 responses are inconsistent in shape across endpoints.

## Prompt

Analyze the current state of 400/validation responses across the API surface I specify: pull actual response bodies (or the code producing them) from several existing endpoints and compare their shapes — some may use the framework's automatic `ValidationProblemDetails` from `[ApiController]` model binding, others may return ad hoc anonymous objects or plain strings. Identify every distinct shape currently in use so the standardization plan accounts for all of them, not just the most common one.

Propose the standardization before implementing: confirm the target shape is RFC 7807 `ProblemDetails`/`ValidationProblemDetails` (type, title, status, detail, instance, plus the `errors` dictionary for field-level validation failures), whether `type` should link to actual documentation or a stable identifier per error category, whether a `traceId`/correlation ID extension member should be included for support purposes, and the migration approach — will this be enforced centrally (a global exception handler plus consistent `ModelState`-to-`ValidationProblem` conversion for `[ApiController]` actions and equivalent handling for minimal API `Results.ValidationProblem`), or does it require touching each endpoint's ad hoc error-construction code individually. Flag any existing client that may depend on the old, inconsistent shape as a backward-compatibility risk before changing it.

Once approved, implement:
- For `[ApiController]` actions, rely on automatic model validation producing `ValidationProblemDetails` by default, and ensure `InvalidModelStateResponseFactory` (if customized) still produces the standard shape.
- For minimal APIs and any manual validation (FluentValidation, custom guard clauses), construct the response via `Results.ValidationProblem(errors)` or `TypedResults.ValidationProblem` rather than hand-rolled objects.
- Replace every ad hoc error shape identified in analysis with the standard one, preserving the actual validation messages/field names being reported.
- Ensure the `Content-Type` is `application/problem+json` consistently.

Write or update tests asserting the exact ProblemDetails shape (status, title, and an `errors` entry per invalid field) for representative endpoints across the affected surface. Confirm with me before changing the response shape of any endpoint with known external consumers, since this is technically a breaking change for clients parsing the old shape.
