# Add or Expand Swagger/OpenAPI Documentation

**Category:** ASP.NET Core
**Use when:** an API needs discoverable, accurate docs for consumers/Swagger UI.

## Prompt

Analyze the current Swagger/OpenAPI setup: whether `Swashbuckle.AspNetCore` (or `Microsoft.AspNetCore.OpenApi`) is already configured, what level of detail existing endpoints already document (summaries, response types, examples), and whether XML doc comments are enabled and flowing into the generated spec (`GenerateDocumentationFile` in the csproj plus `IncludeXmlComments` in `AddSwaggerGen`).

Propose the documentation scope before implementing: which endpoints/controllers are in scope for this pass, what needs adding for each — summary/description, `[ProducesResponseType]` for every realistic status code the endpoint returns (200/201, 400, 401/403, 404, 409, 500 as applicable, not just the happy path), request/response examples via `IExamplesProvider`/`SwaggerRequestExample`/schema filters consistent with whatever example mechanism the project already uses, and whether any endpoints need to be explicitly hidden from the public spec (`[ApiExplorerSettings(IgnoreApi = true)]`) because they're internal-only.

Once approved, implement:
- Add or correct `[ProducesResponseType]`/`Produces`/`Consumes` attributes so the generated spec accurately reflects real behavior — never document a status code the endpoint can't actually return, and never omit one it does return.
- Add XML doc comments (`/// <summary>`) for the endpoint methods and DTO properties being documented, matching existing comment style.
- If the project uses `[SwaggerOperation]`/`[SwaggerResponse]` attributes instead of or alongside XML comments, follow that existing pattern rather than introducing a second documentation mechanism.
- Ensure security requirements (JWT bearer, API key) are correctly reflected in the generated spec (`AddSecurityDefinition`/`AddSecurityRequirement`) so Swagger UI's "Authorize" flow actually works for these endpoints.
- Do not document or expose internal implementation details, secrets, or non-public fields in examples.

Validate by generating the spec (`dotnet build` plus loading the Swagger JSON/UI) and confirm it renders without schema errors and that the documented responses match what the endpoint actually returns for at least the success and one error case. Confirm with me before changing the public route/version structure as a side effect of this documentation pass.
