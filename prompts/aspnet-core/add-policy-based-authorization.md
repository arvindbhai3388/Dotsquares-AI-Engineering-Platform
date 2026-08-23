# Add Policy-Based Authorization

**Category:** ASP.NET Core
**Use when:** simple role checks aren't expressive enough for the access rule needed.

## Prompt

Analyze the access rule I need enforced and the current authorization setup: existing `[Authorize(Roles = ...)]` usage, any existing custom `IAuthorizationRequirement`/`AuthorizationHandler<T>` implementations already in the codebase to follow for style, and what claims are actually present on the authenticated principal (inspect a sample token/claims principal rather than assuming a claim exists).

Propose the design before implementing: whether this rule is expressible as a simple claims-based policy (`RequireClaim`, `RequireAssertion`) registered directly in `AddAuthorization`, or needs a full custom `IAuthorizationRequirement` + `AuthorizationHandler<TRequirement, TResource>` for resource-based checks (e.g., "user can only edit their own record" or "user must belong to the same tenant as the resource"). Identify exactly which resource lookup the handler needs to perform to make that decision, and whether that lookup introduces a new dependency the handler needs injected (repository/service), and confirm the failure behavior (403 with what body) matches the existing authorization failure handling in the app.

Once approved, implement:
- Register the policy via `AddAuthorizationBuilder().AddPolicy(...)` (or `AddAuthorization(options => ...)`), and register the handler in DI.
- For resource-based checks, invoke `IAuthorizationService.AuthorizeAsync(User, resource, policyName)` explicitly in the endpoint/controller after loading the resource, rather than relying solely on attribute-based checks that run before the resource is loaded.
- Apply `[Authorize(Policy = "...")]` or `.RequireAuthorization("...")` to the correct endpoints only — do not broaden the policy's application beyond what was requested.
- Make sure the handler calls `context.Succeed`/does nothing on failure rather than throwing, per the framework's expected pattern, and never assume a claim exists without null-checking it.

Write or update tests covering: an authorized user succeeding, an unauthorized user (wrong role/claim/ownership) receiving 403, and a resource-based check correctly denying access to another tenant's/user's resource. Confirm with me before changing any authorization rule already protecting production data.
