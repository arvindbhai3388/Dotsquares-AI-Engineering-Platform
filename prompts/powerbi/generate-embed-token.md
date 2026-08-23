# Generate a Power BI Embed Token Server-Side

**Category:** Power BI
**Use when:** Adding embedded report support to a .NET app for the first time.

## Prompt

Implement server-side generation of a Power BI embed token for a specific report and workspace in this .NET application. Before writing code, follow the analyze -> propose -> approve -> implement -> test -> review workflow: locate the existing HTTP client/service layer conventions, configuration/options pattern, and DI setup already used in this project, then propose the smallest change that fits them rather than introducing a new framework.

Requirements for the implementation:
- Authenticate against Azure AD using a service principal (app registration with a client secret or certificate) via the Microsoft Authentication Library (MSAL), not a hardcoded master user account. Never hardcode the tenant ID, client ID, client secret, workspace ID, or report ID in source — read them from the existing configuration/options pattern (strongly typed options class bound from config), and if a config file is restricted or off-limits, ask me for a placeholder value instead of opening it.
- Call the Power BI REST API's GenerateToken endpoint (POST /v1.0/myorg/groups/{groupId}/reports/{reportId}/GenerateToken) using the official Power BI .NET SDK client or a typed HttpClient, and return the embed token, embed URL, and report ID to the caller in a single DTO.
- Handle embed token expiry explicitly: Power BI embed tokens are short-lived (typically ~60 minutes). Document the expiration in the response DTO (the token response includes an "expiration" field) so the frontend can proactively refresh before it lapses, rather than waiting for a failed render.
- Handle and surface Power BI API throttling (HTTP 429) and authentication failures (401/403) with clear, non-leaking error messages -- never log the client secret, access token, or embed token value itself; redact them in any logging.
- Add appropriate exception handling around the AAD token acquisition step separately from the Power BI API call step, since failures at each stage have different causes and remediations.
- Write unit tests (or propose them first per Test-First) covering: successful token generation, AAD auth failure, Power BI API error response, and missing/invalid configuration.

Do not scaffold an entire embedding UI in this task -- scope it strictly to the server-side token generation endpoint/service. Stop and ask me before introducing any new NuGet package beyond the official Power BI/MSAL client libraries.
