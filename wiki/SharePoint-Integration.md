# SharePoint Integration

Guidance for integrating with SharePoint Online via Microsoft Graph, the platform's default approach over the legacy CSOM/SharePoint REST APIs.

## Microsoft Graph auth models — app-only vs. delegated

| | App-only | Delegated |
|---|---|---|
| Acts as | The application itself, with its own permissions | The signed-in user, constrained to what that user can access |
| Auth flow | Client credentials flow (`IConfidentialClientApplication.AcquireTokenForClient`) using a client secret or certificate | Authorization code flow (interactive sign-in) or On-Behalf-Of flow (service-to-service, preserving a user's identity) |
| Permission type in Azure AD | Application permissions (e.g., `Sites.Read.All` as Application) — require admin consent | Delegated permissions (e.g., `Sites.Read.All` as Delegated) — consented by the signed-in user or an admin on their behalf |
| Typical use case | Background jobs, migrations, indexing/sync services, anything running unattended with no signed-in user | Interactive web apps where actions should be attributed to and scoped by the actual user (e.g., "show me the files I have access to") |
| Access scope | Can reach every site/list/drive the granted application permission covers, tenant-wide by default unless restricted | Naturally scoped to exactly what that user could do themselves in SharePoint's own UI |
| Risk profile | Broad by default — an app-only credential with `Sites.ReadWrite.All` can touch every site in the tenant unless narrowed | Narrower by default, but ties functionality to a live, valid user session/token |

- **Prefer delegated auth** whenever a real user is present in the flow and the feature is naturally "do this on behalf of the signed-in user" — it keeps the effective permission boundary equal to the user's own SharePoint permissions, which is both more secure by default and usually what the feature actually needs.
- **Use app-only auth** only for genuinely unattended scenarios (a nightly sync job, a webhook processor with no user context) and, when possible, narrow its blast radius with **Application Access Policies** (`New-ApplicationAccessPolicy` in SharePoint PowerShell / Graph's application access policy API) restricting the app-only permission to specific sites rather than the whole tenant — `Sites.Selected` is the modern, narrower application permission that requires explicit per-site grants instead of `Sites.ReadWrite.All`'s tenant-wide default, and should be the default choice for new app-only integrations.
- Never fall back to app-only auth purely to avoid handling an interactive sign-in flow in a user-facing feature — that trade broadens the app's effective access far beyond what the feature needs, purely for developer convenience.

## Throttling and retry patterns

- Microsoft Graph enforces **per-app, per-tenant throttling limits** that are not published as a fixed static number — they vary by workload and current service load. Any Graph integration must handle `429 Too Many Requests` (and transient `503`/`504`) as an expected, routine condition, not an exceptional failure.
- Always honor the **`Retry-After`** header on a `429`/`503` response — wait at least that long before retrying, rather than retrying immediately or on a fixed short interval that will likely just be throttled again.
- Implement retry with **exponential backoff and jitter** for transient errors generally (not just ones carrying `Retry-After`), capped at a small number of attempts (3–5 is typical) before surfacing a failure — Polly's `WaitAndRetryAsync` with a jittered backoff policy is the standard building block for this in .NET, and is already a platform dependency for `AsContinuousMonitoringWorker`-style background services elsewhere in Dotsquares codebases.
- Use **`$batch`** requests to combine multiple independent Graph calls into a single HTTP round trip when a feature needs several related reads/writes together — this reduces both latency and the number of requests counted against throttling limits.
- For bulk/large-scale operations (migrating thousands of files, enumerating a large document library), use **delta queries** (`GET /sites/{id}/drive/root/delta`) to fetch only what changed since the last sync rather than re-enumerating everything on every run — this is both dramatically more throttling-friendly and the correct pattern for building a sync service in the first place.
- Do not implement custom retry logic ad hoc per call site — centralize it in the `HttpClient`/`GraphServiceClient` pipeline (a Polly-based `DelegatingHandler`, or the Graph SDK's built-in `RetryHandler`, which is included by default in the `Microsoft.Graph` client pipeline) so every Graph call benefits consistently.

## Common site/list/drive operations

- **Sites**: resolve a site by its hostname+path (`GET /sites/{hostname}:/{server-relative-path}`) rather than hardcoding a site ID, which can differ between environments (dev/test/prod tenants) even for "the same" site.
- **Lists**: `GET /sites/{site-id}/lists/{list-id}/items?expand=fields` to read list items with their custom columns; filter server-side with `$filter` on indexed columns where possible rather than pulling the full list and filtering client-side, especially on lists with more than a few thousand items (SharePoint's list view threshold applies to Graph-driven queries too).
- **Drives/files**: `GET /drives/{drive-id}/root:/{path}:/children` to list a folder's contents; `PUT /drives/{drive-id}/root:/{path}:/content` for small file uploads (under 4 MB) and the **resumable upload session** API (`POST .../createUploadSession`, then chunked `PUT`s) for anything larger — do not attempt a single `PUT` for large files, which will fail outright above Graph's simple-upload size limit.
- **Permissions**: prefer `POST /drives/{drive-id}/items/{item-id}/invite` (or the equivalent list/site permission endpoints) to grant access explicitly and auditable, rather than relying on inherited permissions being "close enough" for a feature with specific sharing requirements.
- Always request only the fields actually needed via `$select` — a Graph query without `$select` returns a large default field set per item, which matters both for payload size and for throttling cost on list-heavy operations.

## CSOM vs. Graph guidance

- **Default to Microsoft Graph** for all new SharePoint integration work. It is Microsoft's actively invested-in API surface for Microsoft 365 generally, works uniformly across SharePoint/Teams/OneDrive/Outlook without a SharePoint-specific client library, and does not require the on-premises-oriented CSOM assembly (`Microsoft.SharePoint.Client`) or its throttling/connection-management quirks.
- **CSOM remains necessary** for a narrow set of capabilities Graph does not yet fully cover — certain site-collection administration operations, some classic SharePoint workflow/feature activation scenarios, and a handful of legacy on-premises SharePoint Server integrations (SharePoint Server does not expose Graph at all; Graph is Microsoft 365/SharePoint Online-only). If a client project targets on-premises SharePoint Server rather than SharePoint Online, CSOM (or the SharePoint REST API) is the only option, not a legacy fallback.
- Do not mix CSOM and Graph for the same operation/entity within one feature without a specific reason — pick one per integration surface so auth, retry, and error handling stay consistent; a codebase that uses both for overlapping operations tends to accumulate two parallel and inconsistent sets of retry/error-handling logic.

## Related pages

- [Security Guidelines](../docs/Security-Guidelines.md) — least-privilege Graph scope selection.
- [Power BI Integration](PowerBI-Integration.md) — the analogous service-principal pattern for another Microsoft 365 workload.
- [Architecture Overview](Architecture-Overview.md)
