# Add Output Caching to a High-Traffic Umbraco Page

**Category:** Umbraco CMS
**Use when:** A heavily visited page or partial is causing unnecessary render/database load.

## Prompt

A specific Umbraco page or partial view is under heavy traffic and re-rendering unnecessarily on every request. I need caching added at the correct layer without introducing stale-content bugs for editors. First locate the controller/route (standard template route, a Surface Controller, or a custom `RenderController` override) and the view/partial in question, and check what caching already exists in the codebase (output cache profiles, `HybridCache`/`IAppPolicyCache`/`IMemoryCache` usage, or existing `Cache-Control` headers) so we extend an existing pattern instead of introducing a second caching mechanism.

Propose a plan that specifies:
1. The caching layer: ASP.NET Core Output Caching / Response Caching middleware for the whole page response, versus in-memory caching (`AppCaches.RuntimeCache` or `IAppPolicyCache`) around a specific expensive data lookup (e.g., an Examine query or an external API call inside the view/controller), versus donut/fragment caching for a partial that varies by request but sits on an otherwise cacheable page.
2. Cache key design, including variance by culture/domain if this is a multi-language or multi-site setup, and by querystring/route values if the page has filters.
3. **Cache invalidation tied to Umbraco's publishing workflow** -- this is the critical edge case: the cache must be invalidated (or set to a short TTL) on `ContentPublishedNotification`/`ContentUnpublishedNotification`/`ContentCacheRefresherNotification` for the relevant content, not left to expire on a blind timer, or editors will publish changes and see stale content.
4. Any effect on personalized or member-restricted content -- do not cache pages that vary by logged-in member state without keying on that.

Wait for my approval. On implementation, add the chosen caching mechanism, wire the notification handler for cache invalidation, and set a sane fallback TTL as a safety net. Validate by publishing a content change and confirming the front end reflects it within the expected invalidation window, and confirm the cache correctly serves cached output on repeat requests (verify via response headers or a temporary timestamp marker, then remove the marker before calling this done).
