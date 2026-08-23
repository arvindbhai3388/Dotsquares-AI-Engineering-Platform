# Enable Response Compression

**Category:** ASP.NET Core
**Use when:** large JSON payloads are increasing bandwidth or latency.

## Prompt

Analyze the current response pipeline: whether the API is served over HTTPS (compression combined with encrypted traffic is generally safe from BREACH-style attacks, but confirm whether any endpoint reflects secret/sensitive tokens in the response body alongside attacker-influenced input, which is the specific scenario compression can make risky), whether a reverse proxy or CDN in front of this service (e.g., IIS, Nginx, Azure Front Door, Cloudflare) already performs compression — enabling it twice wastes CPU for no benefit — and what content types/response sizes are actually large enough to justify compression overhead.

Propose the configuration before implementing: which MIME types to enable beyond the framework defaults (confirm `application/json` is included, since it isn't always the default depending on framework version), the compression provider(s) — Brotli and/or Gzip — and their compression level (favor a faster level for dynamically generated API responses over maximum compression, which adds latency), whether compression should be limited to responses above a minimum size threshold, and confirmation that this isn't being enabled redundantly with an upstream proxy.

Once approved, implement:
- Register `AddResponseCompression` in Program.cs with the appropriate providers and MIME types, and call `UseResponseCompression()` early in the middleware pipeline (before `UseStaticFiles`/routing, per the framework's documented ordering).
- Set explicit `CompressionLevel` per provider rather than relying on defaults if latency is a concern.
- If HTTPS is in use, confirm response compression won't be combined with any endpoint that reflects user-controllable secrets in a way that creates a compression-oracle risk (e.g., reflecting a CSRF token or session identifier alongside attacker-controlled query data) — flag any such endpoint rather than blanket-enabling compression everywhere.
- Verify compression doesn't break existing `Content-Length`-dependent client logic (compressed responses use chunked transfer/`Content-Encoding` instead).

Write or update tests/manual verification confirming the `Content-Encoding` header appears on eligible responses, payload size actually decreases, and previously-passing client integration tests still pass with compression enabled. Confirm with me before enabling this on any endpoint that reflects attacker-influenced content next to sensitive values.
