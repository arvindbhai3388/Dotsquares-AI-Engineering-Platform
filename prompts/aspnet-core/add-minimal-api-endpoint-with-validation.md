# Add a Minimal API Endpoint With Validation

**Category:** ASP.NET Core
**Use when:** adding a new minimal API route that needs input validation and consistent error responses.

## Prompt

Analyze the existing minimal API setup in this project (Program.cs endpoint groups, MapGroup usage, existing route conventions, DI registrations, and how other endpoints in the same feature area validate input and shape responses). Then propose an approach before writing any code: the route template and HTTP verb, the request/response DTOs (including nullability annotations), whether validation should use data annotations, a manual guard clause, or a FluentValidation validator consistent with what's already used elsewhere in the codebase, and how errors will map to a ProblemDetails response via `Results.ValidationProblem` or `IResult` with the correct status code.

Once I approve the approach, implement the endpoint:
- Register it with `MapGet`/`MapPost`/etc. on the appropriate `RouteGroupBuilder`, following existing grouping/versioning conventions.
- Bind the request via `[AsParameters]`, route/query parameters, or a JSON body as appropriate, with explicit nullable reference type annotations.
- Validate input server-side even if the client is expected to validate — never trust the request body or query string.
- Return typed results (`Results<Ok<T>, ValidationProblem, NotFound>` or equivalent) rather than untyped `IResult` where the project already does this.
- Accept and propagate a `CancellationToken` from the request pipeline into any downstream async calls.
- Handle the not-found, unauthorized, and malformed-input paths explicitly, not just the happy path.
- Avoid leaking internal exception details or stack traces in the response body.

Then write or update tests covering: valid input succeeding, invalid input returning the expected ProblemDetails shape and status code, missing/null required fields, and any authorization boundary the endpoint has, using the project's existing test project and `WebApplicationFactory` if integration-level coverage is warranted. Stop and confirm with me before touching shared middleware, filters, or the DI container setup in Program.cs if the change would affect other endpoints.
