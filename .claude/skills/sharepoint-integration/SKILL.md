---
name: sharepoint-integration
description: >
  Use when adding a new Microsoft Graph/SharePoint integration point to a
  .NET application — covers choosing the auth model, handling
  throttling/retry, and scoping permissions to least privilege. Trigger
  phrases: "add a SharePoint integration", "connect this app to
  SharePoint via Graph", "set up Graph auth for this feature". For general
  fixes to already-wired integrations, prefer the sharepoint-developer
  agent; use this skill when adding a new integration point end to end.
---

# SharePoint/Graph Integration Workflow

The two decisions that matter most here — auth model and permission
scope — are the hardest to change after the fact (they involve tenant
admin consent), so this workflow front-loads them.

## Step 1 — Determine the auth model

Ask: does this integration act **on behalf of a signed-in user**, or
**unattended, as the app itself**?

- **Delegated** (on behalf of a user): use when the feature is genuinely
  "show/act on what this user can access" — a user-facing "browse my
  SharePoint files" feature, an action attributed to the actual user for
  audit purposes. Requires an interactive or on-behalf-of sign-in flow
  and per-user (or admin) consent to delegated scopes.
- **App-only** (service principal / client credentials): use for
  background/unattended work with no signed-in user context — a
  scheduled sync job, a webhook handler processing events for any
  tenant user. Always requires admin consent for the application
  permissions granted, and the app can then act across whatever scope
  those permissions allow, independent of any specific user.
- Don't default to app-only just because it's simpler to wire up — if the
  feature is fundamentally user-facing and should be scoped/audited per
  user, delegated is the correct choice even though it requires more
  setup.

## Step 2 — Scope permissions to least privilege

- List the specific Graph operations the feature actually needs (read a
  specific list, write to a specific document library) before picking a
  permission — don't start from "what permission would definitely work"
  (usually an `.All`-suffixed one) and work backward.
- Prefer scoped permissions where they exist for the resource type (e.g.,
  `Sites.Selected` restricted to specific sites via a follow-up Graph
  call granting the app access to just those sites) over tenant-wide
  `.All` permissions (`Sites.ReadWrite.All`), unless the feature
  genuinely requires tenant-wide access — if it does, document why
  explicitly, since this is exactly the kind of decision a later security
  review will (and should) question.
- Application permissions always need tenant admin consent — confirm this
  is understood and planned for (who requests/grants consent, in which
  environment) rather than discovering it only when deployment fails.

## Step 3 — Set up the Graph client with resilience built in

- Construct the client via the SDK's standard auth provider
  (`ClientSecretCredential`/`ClientCertificateCredential` for app-only,
  or the appropriate delegated flow) from
  `Azure.Identity`/`Microsoft.Identity.Client` — don't hand-roll OAuth
  token acquisition.
- Confirm the SDK's default retry handler (honoring `Retry-After` on
  `429`/`503`) is actually present in the client's message-handler
  pipeline if the client construction customizes handlers — don't strip
  it while adding custom handlers (logging, telemetry) without re-adding
  equivalent retry behavior.
- For bulk operations, use `$batch` or the SDK's page iterator rather
  than looping individual requests — this both reduces throttle-countable
  request volume and simplifies pagination handling.

## Step 4 — Implement the operation

- Resolve site/list/drive IDs once and reuse/cache them for the session
  rather than re-resolving by path on every call, if the same
  site/list/drive is accessed repeatedly.
- Request only the fields needed (`$select`) rather than full item
  payloads.
- For file uploads over 4MB, use an upload session
  (`CreateUploadSession`), not a single PUT.

## Step 5 — Never wire real tenant credentials into this repo

- This platform's demos must use a mocked/stubbed Graph client behind the
  same interface a real integration would use (platform CLAUDE.md §4) —
  build (or reuse) an `ISharePointClient`-style abstraction the real
  Graph SDK client implements, with a mock/stub implementation for demo/
  test purposes.
- Bind tenant ID, client ID/secret or certificate, and any
  site/list/drive identifiers through configuration — never hardcode, and
  never commit a real value anywhere in this repo, including demo
  `appsettings.Development.json` or test fixtures.

## Step 6 — Test and validate

- Unit-test the integration logic against the mock/stub client,
  exercising both success and throttled/failure responses (confirm retry
  behavior actually triggers and eventually gives up cleanly).
- If a real sandbox tenant is available for manual verification (outside
  this repo's own automated tests), verify least-privilege scoping there
  too — confirm the app genuinely cannot access resources outside its
  granted scope, not just that it can access what it's supposed to.
- Run `build-validator` before calling the integration done.

## Do
- Decide delegated vs app-only based on who's actually acting.
- Scope permissions to the narrowest that satisfies the need; document
  any `.All` permission's justification explicitly.
- Preserve the SDK's built-in retry/throttling behavior.
- Use mock/stub clients in this repo; never real tenant credentials.

## Don't
- Don't default to app-only/`.All` permissions for convenience.
- Don't hand-roll retry logic that ignores `Retry-After`.
- Don't hardcode tenant/client identifiers or secrets anywhere.
- Don't claim an integration works without exercising it against a mock
  (and a real sandbox, if available) covering both success and failure
  paths.
