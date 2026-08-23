# Refactor a Fat Controller Into a Thin Controller Plus Service Layer

**Category:** ASP.NET Core
**Use when:** a controller has accumulated business logic that should be independently testable.

## Prompt

Analyze the controller I specify: enumerate every action method, and for each one classify its logic into HTTP concerns (model binding, status code selection, routing) versus business/domain logic (validation rules beyond input shape, orchestration across multiple data sources, calculations, conditional business rules). Identify any direct `DbContext`/data-access calls, external API calls, or file/IO operations currently sitting inline in the controller, and check whether a service layer convention already exists elsewhere in the codebase (interface naming, DI registration lifetime, namespace/folder structure) that this refactor should match rather than inventing a new pattern.

Propose the extraction plan before implementing: the new service interface(s) and their method signatures (inputs/outputs, `async Task<T>` with `CancellationToken` parameters), which controller action maps to which service method, how existing exceptions/error signaling translate (does the service throw domain exceptions that the controller or a global exception handler maps to status codes, or return a result type?), and confirm this preserves the exact existing HTTP contract (routes, status codes, response shapes) since this is a refactor, not a behavior change.

Once approved, implement:
- Create the service interface and implementation, moving business logic over method-by-method, preserving behavior exactly.
- Register the service in DI with an appropriate lifetime (typically scoped, matching how similar services are registered).
- Reduce each controller action to: bind input, call the service, map the result/exception to an `IActionResult`/status code — nothing else.
- Keep authorization attributes and model validation on the controller where they belong; move only business logic.
- Do not change route templates, status codes, or response shapes as a side effect — flag anything that looks like it should change but wasn't asked for.

Write or update tests: unit tests against the new service directly (mocking its dependencies) covering the business logic branches that were previously untestable without spinning up the full HTTP pipeline, and confirm existing controller-level tests still pass unchanged since the external contract didn't move. Confirm with me before changing anything beyond the extraction itself.
