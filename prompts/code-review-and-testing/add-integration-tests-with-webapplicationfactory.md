# Add Integration Tests with WebApplicationFactory

**Category:** Code Review & Testing
**Use when:** Unit tests alone don't cover the full request pipeline (middleware, DI, routing).

## Prompt

Add integration tests for the Web API endpoint(s) I specify, using `WebApplicationFactory<TEntryPoint>` so the tests exercise the real request pipeline -- routing, middleware, filters, model binding, and DI container wiring -- rather than calling the controller method directly in-process.

Set this up properly:

1. Create (or extend) a custom `WebApplicationFactory` subclass that overrides `ConfigureWebHost` to replace only the dependencies that must not touch real infrastructure in a test run -- typically the database context/connection and any outbound HTTP clients to third parties. Use an in-memory or test-scoped database (or a fake/stub repository behind the existing interface) rather than pointing at a real database. Leave everything else (routing, filters, DI graph, middleware pipeline) running as configured in the real `Startup`/`Program` so the test is actually exercising integration behavior.
2. Use `IClassFixture<CustomWebApplicationFactory>` so the factory is shared across tests in a class but each test still gets an isolated `HttpClient` via `factory.CreateClient()`.
3. Write tests that send real HTTP requests (`GetAsync`, `PostAsJsonAsync`, etc.) and assert on the actual `HttpResponseMessage` -- status code, response body shape/content, and relevant headers (e.g., `Location` on a 201, `WWW-Authenticate` on a 401).
4. Cover: success path with a valid request; validation failure returning 400 with the expected error shape; unauthorized/forbidden paths if the endpoint requires auth (test both missing and insufficient-role tokens); not-found paths for resource lookups by ID; and, if applicable, idempotency or duplicate-request behavior.
5. Ensure test data setup/teardown makes each test independent -- reset or scope the in-memory store per test class or per test so ordering never matters and tests can run in parallel without interfering with each other.

Do not assert on internal implementation details (e.g., exact SQL executed, private field state) -- integration tests should verify externally observable behavior only. After writing the tests, run `dotnet test` on the actual test project and report real results. If the project doesn't yet have a test project set up for WebApplicationFactory-style tests, tell me before creating one, matching the existing test project's package/test-runner conventions.
