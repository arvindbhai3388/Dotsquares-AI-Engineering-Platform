# Diagnose and Fix Embed Token Expiry Mid-Session

**Category:** Power BI
**Use when:** Users report "This content isn't available" errors mid-session.

## Prompt

Diagnose why users see "This content isn't available" or similar embed failures after leaving a Power BI report open for an extended period, and fix the root cause. Treat this as a debugging task: reproduce and confirm the mechanism before changing code, rather than assuming it's the token-expiry issue without verifying.

Investigation steps:
1. Locate the existing embed-token generation code and the frontend embedding code (see the app's embed-in-page implementation) and confirm the embed token's actual lifetime as returned by Power BI's GenerateToken response (`expiration` field) -- standard embed tokens are typically valid for about an hour, materially shorter than a typical "dashboard left open" session.
2. Check whether any proactive refresh logic exists at all (a scheduled `setTimeout`/timer that re-fetches a token and calls `report.setAccessToken()` before expiry) -- if it's missing entirely, that's very likely the root cause and the fix is to add it (see the dedicated embed-with-token-refresh prompt for the full pattern) rather than a deeper Power BI-side issue.
3. If refresh logic exists, check for these specific failure modes before assuming it's broken: (a) browser tab throttling of `setTimeout` in backgrounded tabs causing the scheduled refresh to fire late or not at all -- verify whether the code also listens for the SDK's `tokenExpired` event as a fallback; (b) the refresh endpoint itself failing (expired AAD app credentials, service principal removed from workspace, revoked API permissions) -- check server-side logs for the token-refresh call specifically, not just the initial embed; (c) a race condition where `setAccessToken` is called with a token for the wrong report/dataset if multiple reports are embedded on the same page.
4. Distinguish this from unrelated causes that produce similar user-facing errors: workspace capacity paused, dataset/report deleted or moved, RLS role misconfiguration returning zero rows (different symptom, no data rather than a hard error), or a network/firewall issue between the browser and Power BI's CDN.

Once the root cause is confirmed, propose the fix, get approval, then implement it -- typically adding or hardening the proactive-refresh-plus-`tokenExpired`-fallback pattern. Add a regression test or manual verification step (e.g. artificially shortening the refresh window in a test environment) so this doesn't silently regress.
