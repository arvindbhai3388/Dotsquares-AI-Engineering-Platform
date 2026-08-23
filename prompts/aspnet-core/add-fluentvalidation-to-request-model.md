# Add a FluentValidation Validator for a Request Model

**Category:** ASP.NET Core
**Use when:** a request model needs validation rules beyond simple data annotations.

## Prompt

Locate the request DTO I specify and analyze its current validation: data annotation attributes already present, any existing `AbstractValidator<T>` classes elsewhere in the codebase I should follow for naming/registration conventions, and how FluentValidation (if already referenced) is wired into the pipeline — via `AddFluentValidationAutoValidation`, manual `IValidator<T>.ValidateAsync` calls in the action/endpoint, or a pipeline behavior if MediatR is in use.

Propose the validation rules before implementing: enumerate every field and its constraints (required, length, format, range, cross-field rules like "EndDate must be after StartDate", conditional rules based on another field's value), which rules need async validation (e.g., checking uniqueness against the database) versus which are pure in-memory checks, and confirm the error message wording/format matches what the API already returns elsewhere so validation errors are consistent across endpoints.

After I approve, implement:
- Create the `AbstractValidator<T>` class in the same folder/namespace convention as existing validators.
- Use `RuleFor`/`When`/`Must`/`MustAsync` appropriately; for async DB-backed rules, accept a `CancellationToken` and inject the minimum dependency needed (repository/service interface, not a raw `DbContext` if the codebase doesn't do that elsewhere).
- Register the validator in DI (`AddScoped<IValidator<T>, ...>` or `AddValidatorsFromAssembly`) matching existing registration style.
- Wire it into the actual request pipeline so it runs before the handler executes, and confirm the resulting error response still matches the API's existing ProblemDetails/error shape.
- Do not duplicate rules already enforced by data annotations unless intentionally replacing them — decide and document which validation mechanism is now the source of truth for this model.

Write or update unit tests for the validator directly (valid input, each rule's failure case, boundary values) plus an integration/controller-level test confirming invalid input produces the expected HTTP response. Confirm with me before removing any pre-existing data annotation attributes.
