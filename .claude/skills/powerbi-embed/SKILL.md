---
name: powerbi-embed
description: >
  Use when wiring up a new Power BI embedded report into a .NET
  application — service principal auth, embed token generation, and
  row-level security (RLS) setup. Trigger phrases: "embed a Power BI
  report", "set up Power BI embedding for this app", "add RLS to this
  embedded report". For general fixes to already-wired embedding code,
  prefer the powerbi-developer agent; use this skill when standing up a
  new embed integration end to end.
---

# Power BI Embed Setup Workflow

This walks through embed-for-your-customers (app-owns-data) setup — the
pattern almost all client SaaS-style embedding scenarios need — with RLS
designed in from the start rather than retrofitted.

## Step 1 — Confirm prerequisites before writing code

- **Capacity**: the target workspace must be on a Premium/Fabric capacity
  (or Premium Per User, per current licensing) — app-owns-data embedding
  via service principal doesn't work against a shared/Pro-only workspace.
  Confirm this is provisioned before debugging embed failures as if
  they're code bugs.
- **Service principal**: an Azure AD app registration exists, is added to
  a Power BI security group enabled for API access (via the Power BI
  admin portal's tenant settings), and has been granted access to the
  target workspace as a member/contributor. Confirm this chain, not just
  that "an app registration exists somewhere."
- **RLS model**: confirm whether the `.pbix` report's dataset already has
  RLS roles defined (built in Power BI Desktop) — if not, this must be
  designed and added to the model before the embed integration can
  enforce per-customer/per-user data scoping; flag this as a prerequisite
  task if it's missing and the feature requires row-level scoping (most
  multi-tenant embeds do).

## Step 2 — Design the identity mapping

- Decide how the app's authenticated user/tenant maps to the Power BI RLS
  role and username/custom-data value the embed token will carry. This
  mapping **is** the security boundary for a multi-tenant embed — get it
  explicit and auditable (e.g., "app tenant ID maps 1:1 to RLS username
  claim `TenantId`, enforced by an RLS filter on the `Tenant` table in
  the model").
- Do not design a mapping that lets the calling application choose an
  arbitrary RLS identity per request without server-side validation that
  the caller is actually entitled to that identity — the backend service
  issuing embed tokens must derive the RLS identity from its own
  authenticated session state, never from a client-supplied parameter
  taken at face value.

## Step 3 — Implement server-side embed token generation

- Use the service principal's client credentials
  (`Microsoft.Identity.Client`/`Azure.Identity` for auth,
  `Microsoft.PowerBI.Api` for the Power BI REST calls) to acquire an
  Azure AD token, then call the Power BI "Generate Token" API for the
  target report/dataset, passing an `EffectiveIdentity` block built from
  Step 2's mapping (username/role(s), and dataset ID(s)).
- Never expose the service principal's client secret/certificate to any
  client-side code — token generation is a server-side endpoint only; the
  client receives just the resulting short-lived embed token and
  embed URL.
- Bind the service principal's tenant ID, client ID/secret, workspace ID,
  and report/dataset IDs through configuration/options — never hardcode
  them, and for this repo specifically, never write real values into any
  file (demos use placeholders/mocks per platform CLAUDE.md §4).
- Generate a fresh token per embedding session/user context — never cache
  and reuse a token generated for one user's RLS identity for a different
  user.

## Step 4 — Wire up the client-side embed

- Use the `powerbi-client` JS SDK (or the project's existing wrapper
  around it) to embed using the token + embed URL from Step 3.
- Handle the SDK's `tokenExpiration` event to request a fresh token from
  the backend before the current one lapses, rather than letting the
  embedded report silently fail mid-session.

## Step 5 — Verify RLS actually enforces, don't just assume the model is right

- Generate embed tokens for **two different** simulated identities/roles
  and confirm the returned report data genuinely differs between them —
  a model that "looks" RLS-configured but was never exercised through the
  real token-generation path is not verified. This is the single most
  important verification step in this workflow; a broken or missing RLS
  identity on the token can silently return unfiltered (all-tenant) data.
- Confirm a request **without** a valid RLS identity (if that path is
  reachable at all) is rejected or scoped to nothing, not silently
  returning unfiltered data.

## Step 6 — Handle throttling and failures

- Wrap Power BI REST API calls with retry-with-backoff (Polly, if already
  a project dependency) honoring `429`/`Retry-After` responses — don't
  fail the whole embed flow on a transient throttle without at least one
  retry.
- Surface a clear, non-leaky error to the client on token-generation
  failure (don't propagate raw Power BI API error details that might
  include internal identifiers) while logging the real error server-side
  without secrets.

## Do
- Confirm capacity, service-principal workspace access, and RLS model
  existence before writing integration code.
- Derive the RLS identity server-side from authenticated session state,
  never from a client-supplied value taken at face value.
- Verify RLS by generating tokens for two identities and diffing the
  actual returned data.

## Don't
- Don't expose service-principal credentials client-side.
- Don't cache an embed token across different users' RLS contexts.
- Don't hardcode tenant/workspace/report/dataset IDs or secrets.
- Don't call RLS "done" without an end-to-end verification against real
  token generation.
