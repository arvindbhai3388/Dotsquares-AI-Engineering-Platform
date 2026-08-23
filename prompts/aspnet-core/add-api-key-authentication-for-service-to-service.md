# Add API Key Authentication for Service-to-Service Calls

**Category:** ASP.NET Core
**Use when:** a machine-to-machine caller doesn't fit OAuth/JWT flows.

## Prompt

Analyze the endpoint(s) that need service-to-service protection: who the caller is (an internal service, a partner integration, a scheduled job), whether any API key mechanism already exists elsewhere in the solution I should reuse or match conventions with, and how the key will be provisioned, stored, and rotated (confirm keys are never hardcoded and are read via configuration/secret store — never place actual key values in code, comments, or logs during this task).

Propose the design before implementing: the transport (a custom header like `X-Api-Key` is the common convention — confirm or match existing style), how keys map to callers (a single shared key versus per-caller keys with an identifier, which is strongly preferred so a compromised key can be revoked individually and calls can be attributed), the storage/lookup mechanism for valid keys (hashed at rest, not stored in plaintext, similar to password storage practices), and the failure response (401 with no hint about why validation failed, to avoid helping an attacker enumerate valid key formats).

Once approved, implement:
- Implement authentication via a custom `AuthenticationHandler<AuthenticationSchemeOptions>` registered with `AddAuthentication().AddScheme<...>(...)`, or middleware if that better matches existing project conventions, rather than checking the header manually inside each controller.
- Compare presented keys using a constant-time comparison, and validate against hashed stored values, not plaintext.
- Attach the resolved caller identity as claims on the `ClaimsPrincipal` so downstream authorization/logging can attribute the request to a specific caller.
- Apply `[Authorize(AuthenticationSchemes = "ApiKey")]` only to the intended endpoints; do not silently apply it API-wide unless that's genuinely the intent.
- Ensure the key is never logged, including in request-logging middleware, exception messages, or telemetry.

Write or update tests covering: valid key succeeds, missing key returns 401, invalid key returns 401, and (if per-caller keys exist) a revoked/disabled caller's key is correctly rejected. Confirm with me before rotating or invalidating any key already in use by a live integration.
