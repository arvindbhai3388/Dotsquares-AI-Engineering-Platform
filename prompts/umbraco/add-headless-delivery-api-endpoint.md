# Expose Content via the Content Delivery API

**Category:** Umbraco CMS
**Use when:** A separate front end (e.g., a Blazor or JS SPA) needs to consume Umbraco content as JSON.

## Prompt

I need to expose specific content to a decoupled/headless front end using Umbraco's Content Delivery API rather than building a custom API controller from scratch. First confirm the Delivery API is enabled in this installation (it is off by default and requires explicit configuration) and check what's already exposed -- do not duplicate endpoints the Delivery API already provides for the content tree, by Id, or by route.

Propose the plan before implementing:
1. Confirm the Delivery API's built-in endpoints (`/umbraco/delivery/api/v2/content`, content-by-id, content-by-route, media endpoints) cover the requirement as-is versus genuinely needing custom output shaping -- the Delivery API is designed to avoid needing a custom controller for standard content retrieval.
2. Output customization needed via `IApiContentResponseBuilder`/custom `IApiElementBuilder`/property value "Delivery API" converters if specific properties need reshaping for the JSON contract the front end expects (e.g., a Content Picker property should serialize as a nested content reference, not just an Id) -- check whether property editors already have Delivery-API-specific value converters registered before writing a custom one.
3. **Authentication/authorization for the API**: is this content fully public, or does it need API-key-based access control (Delivery API supports a preview/member-based auth mode) -- never expose member-restricted or unpublished content through an unauthenticated endpoint, and confirm the API key configuration lives in the standard options pattern, not hardcoded.
4. Caching strategy for API responses given the Delivery API reads from the published content cache already, and whether an additional HTTP-level cache (CDN/output caching) is warranted for this specific consumer, mirroring how output caching is handled elsewhere in this codebase, with the same publish/unpublish cache-invalidation concern.
5. CORS configuration if the SPA is on a different origin, scoped narrowly to the actual consuming origin(s) rather than a wildcard.

Wait for approval, then implement only the configuration/customization actually needed -- prefer configuration over custom controllers. Validate by calling the endpoint for published content (200 with expected shape), unpublished/non-existent content (404, not a 500 or leaked draft data), and confirm CORS and any API-key enforcement behave correctly from the actual consuming origin.
