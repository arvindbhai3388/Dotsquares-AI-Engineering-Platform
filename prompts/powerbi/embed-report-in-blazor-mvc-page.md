# Embed a Power BI Report with Token Refresh

**Category:** Power BI
**Use when:** Adding an analytics dashboard tab to an existing app.

## Prompt

Embed a Power BI report into an existing page of this application (Blazor component or MVC Razor view, matching whatever this app already uses -- locate the existing page/component conventions before writing anything) using the `powerbi-client` JavaScript library, with proper embed token refresh handling so the report doesn't die mid-session.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow. In the analysis step, identify: the existing server-side embed-token endpoint (or propose one first if it doesn't exist yet -- do not duplicate token-generation logic inline in the view), the app's existing JS bundling/script-inclusion approach, and whether this is a Blazor Server, Blazor WASM, or classic MVC page, since the interop pattern differs for each.

Implementation requirements:
- Load `powerbi-client` from the project's existing static asset pipeline (or via the npm/CDN convention already used elsewhere in the app) -- do not introduce a new frontend build tool just for this.
- On page load, call the server-side embed-token endpoint, then use `powerbi.embed(container, config)` with type "report", the returned embed URL, access token, and token type set to `models.TokenType.Embed`.
- Implement proactive token refresh: read the token's expiration from the server response and schedule a refresh call (e.g. via `setTimeout`) a few minutes before expiry, then call `report.setAccessToken(newToken)` -- do not wait for the report to visibly fail before refreshing, since by then the user has already seen a broken embed.
- Wire up the `tokenExpired` event from the embed SDK as a fallback safety net in case the scheduled refresh didn't fire (tab was backgrounded, timer throttled by the browser, etc.), triggering an immediate re-fetch and `setAccessToken` call.
- Handle embed-level errors (`report.on('error', ...)`) distinctly from token-fetch errors (network/HTTP failures calling your own backend), and show the user a clear "report unavailable" state rather than a blank iframe.
- If this is Blazor, use `IJSRuntime`/JS interop cleanly -- keep the embed/refresh logic in a small dedicated JS module rather than scattering inline script, and dispose the embedded report object when the component is disposed to avoid leaking timers.

Write tests for the server-side token endpoint interaction if not already covered, and manually verify token refresh timing in the browser devtools network tab as part of Validate.
