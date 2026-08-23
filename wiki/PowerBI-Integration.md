# Power BI Integration

Guidance for embedding Power BI analytics into client applications on this platform, using Power BI Embedded / the Power BI REST API's embed-for-your-customers pattern.

## Embedded analytics architecture

The standard "embed for your customers" architecture keeps the application's own users completely unaware of Power BI as a product — they never sign into Power BI directly, never see a Power BI login prompt, and do not need individual Power BI licenses:

```
┌────────────┐     ┌──────────────────┐     ┌───────────────────┐
│  Browser    │────▶│  App backend      │────▶│  Power BI REST API │
│ (embedded   │◀────│ (mints embed      │◀────│  (Azure AD app-    │
│  report)    │     │  tokens on demand)│     │  only auth)        │
└────────────┘     └──────────────────┘     └───────────────────┘
```

1. The browser loads the app page containing an embedded report placeholder (via `powerbi-client` JS SDK or the `PowerBIReportEmbed` React component).
2. The app backend — never the browser directly — authenticates to the Power BI REST API using a **service principal** (an Azure AD app registration with a client secret or certificate), requests the report's embed configuration and a short-lived **embed token**, and returns that token plus the report/embed URL to the browser.
3. The browser's Power BI JS SDK uses the embed token to render the report directly against the Power BI service — report rendering traffic goes browser-to-Power-BI, not through the app backend, so the backend is not a bottleneck for report interactivity once the token is issued.

## Service principal authentication

- Use a **service principal**, not a master user account, for app-owns-data embedding. A master user account (a Power BI Pro/PPU-licensed individual's credentials used programmatically) is Microsoft's legacy pattern, is tied to one human's account lifecycle (breaks when that person leaves or changes their password), and does not scale to unattended production use — new embedding scenarios should use a service principal by default.
- The service principal is an Azure AD app registration granted Power BI's application permissions (via Azure AD app permissions, e.g. `Tenant.Read.All`/`Report.Read.All` as application, not delegated, permissions) and added as a **member of the specific Power BI workspace(s)** it needs to access (as an Admin/Member/Contributor role on that workspace) — do not grant it tenant-wide Power BI admin rights when workspace-level membership is sufficient.
- The service principal's client secret (or, preferably, a certificate for production) is a credential like any other and must never be committed to source — see [Security Guidelines](../docs/Security-Guidelines.md). Store it in Azure Key Vault or the hosting platform's secret store, referenced via configuration, never inlined in `appsettings.json`.
- Enable "Service principals can use Power BI APIs" in the Power BI Admin Portal's tenant settings, scoped to a specific Azure AD security group containing the service principal — not enabled tenant-wide for "any service principal" — to keep this capability auditable and limited.

## Embed token flow

- Embed tokens are **short-lived** (default ~60 minutes, capped at 24 hours for tokens issued for viewing) and scoped to a specific report/dataset — the app backend mints a fresh token per session/report-load rather than caching and reusing one indefinitely across users, both because of the expiry and because a token minted with one user's row-level-security identity (see below) must never be reused for a different user.
- Never send a Power BI **access token** (the service principal's own AAD token used to call the Power BI REST API) to the browser — only the derived, report-scoped **embed token** should ever reach client-side code. The access token has far broader capability (it can call any Power BI REST API the service principal is permitted to) and must stay server-side.
- Set embed token expiry handling on the client: the JS SDK's `tokenExpired` event should trigger the app to request a fresh token from the backend and call `report.setAccessToken()`, rather than letting a long-viewing session's report silently stop refreshing when the token lapses.

## Row-level security (RLS)

- Define RLS **roles** in the Power BI Desktop model (DAX filter expressions on the underlying tables, e.g. `[RegionId] = USERPRINCIPALNAME()`-driven lookup, or more commonly a mapping table joined on an effective identity) before publishing the dataset.
- When minting an embed token for app-owns-data embedding, pass the **effective identity** (`EffectiveIdentity` with `Username` and the target `Roles`) corresponding to the actual application user viewing the report — this is what makes RLS actually filter data per-viewer even though every viewer is, from Power BI's perspective, authenticated as the same service principal.
- The `Username` passed as the effective identity does not need to be a real Power BI/AAD identity — it is an arbitrary string the RLS DAX expressions key off of (commonly the app's own user ID or tenant ID) — but it must be set correctly and cannot be spoofed by the client, since it is set server-side when the embed token is minted, never supplied by the browser.
- Test RLS by minting tokens for a few representative identities and confirming the rendered report actually restricts data as expected — a misconfigured RLS role that silently matches "all rows" for an unrecognized identity is a realistic and serious data-exposure bug, not just a cosmetic one.

## Workspace and capacity planning

- Use **separate workspaces per environment** (Dev/Test/Prod) and, typically, one workspace per logical customer/tenant group for app-owns-data multi-tenant embedding, rather than one workspace holding every client's reports — this keeps permissions, refresh schedules, and capacity assignment cleanly separable per tenant.
- Embedded reports for external/customer-facing use require a **Power BI Embedded capacity (A/EM SKU)** or a Premium capacity (P SKU) assigned to the workspace — a workspace on shared/Pro-only licensing cannot serve app-owns-data embed tokens to end users without individual Power BI licenses.
- Plan capacity SKU by concurrent render load (simultaneous report views), not just total user count — Power BI Embedded capacities are priced/sized by v-cores and have documented concurrency guidance; undersizing shows up as slow report renders and throttling under load, which is worth load-testing before a production launch rather than discovering post-launch.
- Consider **auto-scale** or a pause/resume schedule for embedded capacities used primarily during business hours to control cost, if the client's usage pattern supports it — this is an infrastructure/cost decision worth raising explicitly with the client rather than assuming always-on capacity is required.

## Related pages

- [Security Guidelines](../docs/Security-Guidelines.md) — service principal secret handling.
- [SharePoint Integration](SharePoint-Integration.md) — the analogous Microsoft Graph app-only auth pattern.
- [Architecture Overview](Architecture-Overview.md)
