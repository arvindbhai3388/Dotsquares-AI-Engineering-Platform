# Write an Integration Test for a Hub

**Category:** SignalR
**Use when:** a hub has no automated test coverage.

## Prompt

Write an integration test for the specified Hub that exercises real SignalR wire behavior (method invocation, authorization, group scoping) using WebApplicationFactory<TEntryPoint> with an in-memory TestServer, rather than only unit-testing the Hub class's methods in isolation with mocked IHubCallerClients. Before writing tests, analyze the project's existing test conventions (test project, framework -- xUnit/MSTest, mocking library) and follow them rather than introducing a new pattern; per the test-first discipline, confirm what coverage is genuinely missing before adding tests.

The integration test setup should:
- Use WebApplicationFactory to spin up the app in-memory, then create a real HubConnection pointed at the TestServer's handler via WithUrl(url, options => options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler()) so the test exercises actual SignalR negotiation/transport code, not just a mocked hub instance.
- Override authentication in the test host (e.g., a test authentication handler registered only in the test WebApplicationFactory) so tests can simulate both an authenticated user with specific claims/roles and an unauthenticated connection, to cover authorization behavior without needing a real identity provider.
- Cover at minimum: (1) a successful method invocation with valid input and its expected return value/broadcast, (2) a validation-failure input producing the expected HubException, (3) an authorization-failure case (missing or insufficient claims) resulting in connection rejection or method-level denial as appropriate, and (4) if the hub uses groups, that a client outside the group does not receive a Clients.Group broadcast intended for members only.
- For asserting broadcasts, register a handler via connection.On<T>("MethodName", ...) (or the strongly-typed client interface's equivalent) before invoking the triggering action, and use a TaskCompletionSource with a timeout to await the broadcast rather than an arbitrary Task.Delay, so the test is deterministic and fails fast if the message never arrives.
- Dispose HubConnection instances properly (await connection.DisposeAsync()) in test cleanup to avoid leaking connections across the test run.
- Do not weaken or skip existing tests to make new ones pass; if a genuine behavior gap is found while writing tests, report it rather than silently adjusting the test's expectations to match broken behavior.

After writing the tests, run them for real via the project's test command, confirm they fail for the right reason before any fix is applied (if this is paired with an implementation task), and report pass/fail results explicitly rather than assuming success.
