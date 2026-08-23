# Wire Up JWT Bearer Authentication

**Category:** ASP.NET Core
**Use when:** an API needs token-based authentication.

## Prompt

Analyze the current authentication state of this API: whether `Microsoft.AspNetCore.Authentication.JwtBearer` is already referenced, what identity provider issues the tokens (Azure AD/Entra ID, Auth0, a custom STS, IdentityServer/Duende), what claims the tokens are expected to carry, and where the issuer/audience/signing-key configuration should live — confirm this comes from configuration/options binding and never gets hardcoded or read from a restricted secrets file directly in code.

Propose the setup before implementing: the `TokenValidationParameters` needed (issuer, audience, signing key source — a fixed key, or `Authority` with automatic OIDC metadata/JWKS discovery), whether validation should also check token expiry, `nbf`, and clock skew explicitly, how the app should behave on 401 (WWW-Authenticate header content) versus 403 (authenticated but unauthorized) for existing endpoints, and which endpoints or endpoint groups become `[Authorize]`-protected versus which stay anonymous — enumerate them so nothing is accidentally locked out or accidentally left open.

After I approve, implement:
- Register `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` in Program.cs, binding options from configuration via `IOptions<T>` rather than inline magic strings.
- Add `UseAuthentication()` before `UseAuthorization()` in the middleware pipeline, in the correct order relative to routing.
- Apply `[Authorize]` (or `RequireAuthorization()` for minimal APIs) to the intended endpoints; leave explicitly public endpoints marked `[AllowAnonymous]` so the intent is visible in code.
- Handle token validation failures gracefully — do not leak validation internals in the response.
- Never log raw tokens, and redact the `Authorization` header from any request-logging middleware.

Write or update tests using `WebApplicationFactory` with a test JWT (signed with a test key, not a production secret) covering: valid token succeeds, expired token returns 401, missing token returns 401 on protected routes, and anonymous routes remain accessible without a token. Confirm with me before changing the token issuer/audience configuration for an already-deployed environment.
