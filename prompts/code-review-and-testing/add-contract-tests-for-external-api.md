# Add Contract Tests for an External API

**Category:** Code Review & Testing
**Use when:** The app depends on an external API whose shape could change without notice.

## Prompt

Add contract tests for the external API integration I specify, to catch breaking changes in the third-party API's shape early -- before they surface as a production failure in the code that consumes the response.

Locate the client code that calls the external API (the HTTP client wrapper, the DTOs it deserializes responses into, and any mapping layer that translates the external shape into an internal model). Identify the specific contract assumptions the code depends on: required fields it reads, expected types, enum/string values it branches on, pagination shape, and error response shape for non-2xx responses.

Build the contract tests as one of these, matching whichever fits the team's existing tooling and the external API's nature (pick one and justify the choice, don't set up multiple approaches for the same dependency):

1. **Schema-based contract test** -- capture a real (or realistic, sanitized) example response, define a JSON schema (or equivalent) covering only the fields the code actually depends on, and assert new sample responses still validate against it. Store the sample response as a test fixture, not inline, so it's easy to update deliberately.
2. **Consumer-driven contract test** (if the external API is another internal team's service, e.g., via Pact) -- define the expectations this codebase has of the provider, verify against a mock provider locally, and note that the provider side would need to run the same contract in their pipeline for this to be enforced end-to-end; call out if that provider-side wiring doesn't exist yet, since a one-sided contract test only partially protects you.
3. **Deserialization test against golden fixtures** -- for a simpler integration, keep one or more saved real response payloads as fixtures and assert the client's deserialization/mapping code still produces the expected internal model from each, catching added/removed/retyped fields.

Do not make these tests call the real external API over the network -- they must run offline and deterministically in CI, using saved fixtures or a contract-testing tool's mock provider. Clearly document, in a comment near the fixture, the date and source of the captured sample so a future breaking change is easy to diagnose. After implementation, run the tests and confirm they fail if you deliberately mutate a fixture to remove a field the mapping code depends on -- this proves the contract test would actually catch a real regression, not just always pass.
