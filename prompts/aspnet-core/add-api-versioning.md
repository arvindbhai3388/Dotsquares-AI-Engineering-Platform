# Add API Versioning to an Endpoint Group

**Category:** ASP.NET Core
**Use when:** a breaking change must ship without breaking existing consumers.

## Prompt

Analyze the current API surface for the controller or minimal API group I specify: existing route templates, whether `Asp.Versioning` (or another versioning package) is already referenced, how consumers currently call this API (URL path, headers, query string), and whether any existing endpoints in the solution already carry version information I should mirror for consistency.

Propose a versioning strategy before implementing: URL segment (`/v{version}/...`), header-based (`Api-Version` header), query string, or media-type versioning — pick whichever matches the existing convention in this codebase if one exists, and explain the tradeoff if you're introducing a new pattern. Identify exactly which existing routes become v1 (frozen, unchanged behavior) and which become v2 (the new/breaking behavior), and confirm whether this needs `ApiVersionReader`, `ApiVersionSetBuilder` (for minimal APIs), or `[ApiVersion]`/`[MapToApiVersion]` attributes (for controllers).

Wait for my approval, then implement:
- Wire up versioning services in Program.cs (`AddApiVersioning`, `AddApiExplorer` if Swagger is in use) without disrupting existing unversioned routes until they're migrated.
- Preserve the exact response shape and status codes of the existing (now v1) behavior — this is a backward-compatibility requirement, not optional.
- Implement the new version's behavior in v2 without duplicating unrelated logic; extract shared logic into a common method/service if v1 and v2 diverge only partially.
- Update Swagger/OpenAPI configuration so both versions appear correctly in the generated docs.
- Consider deprecation headers (`Sunset`, `Deprecation`) if v1 is being phased out.

Write or update tests asserting v1 continues returning its original contract byte-for-byte in shape, and v2 returns the new contract, plus a test for requests with no/invalid version specified. Confirm with me before removing or hiding the old version from documentation.
