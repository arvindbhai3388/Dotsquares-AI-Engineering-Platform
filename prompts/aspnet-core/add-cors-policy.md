# Add a Tightly Scoped CORS Policy

**Category:** ASP.NET Core
**Use when:** a new front-end origin needs to call the API.

## Prompt

Analyze the current CORS configuration: whether `AddCors` is already registered in Program.cs, what policies (if any) already exist and what origins/headers/methods they allow, and the exact origin(s) the new client needs (protocol, host, and port — get the precise value from me rather than guessing, and never propose `AllowAnyOrigin` combined with credentials).

Propose the policy before implementing: a named policy scoped to only the origin(s) that need access, only the HTTP methods this client actually calls (not a blanket `AllowAnyMethod` unless genuinely needed), only the headers it actually sends/needs to read (explicit `WithHeaders`/`WithExposedHeaders` rather than `AllowAnyHeader` if the set is known), and whether `AllowCredentials()` is required — if so, confirm this cannot be combined with a wildcard origin (the framework will reject it at runtime, but decide the exact origin list up front rather than discovering this by trial and error).

Once approved, implement:
- Register the named policy via `AddCors(options => options.AddPolicy("...", policy => ...))` with the explicit origin list, methods, and headers agreed above.
- Apply the policy to only the endpoints/controllers that need it via `.RequireCors("...")` or `[EnableCors("...")]`, rather than applying it globally with `UseCors()` defaults unless every endpoint genuinely needs the same policy.
- Ensure `UseCors()` is positioned correctly in the middleware pipeline (after routing, before authorization, per the framework's documented order).
- Do not widen an existing policy's origin list to satisfy this request if a new, separate named policy for the new client is more correct and lower-risk.

Write or update an integration test issuing a preflight (`OPTIONS`) request and a real request from an allowed origin (expect success) and a disallowed origin (expect the CORS headers to be absent so the browser blocks it). Confirm with me before broadening any existing CORS policy that's already relied upon by other clients in production.
