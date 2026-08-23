---
name: powerapps-connector
description: >
  Use when building a new custom Power Platform connector — covers the
  OpenAPI definition, auth configuration, and Dataverse considerations.
  Trigger phrases: "build a custom connector for this API", "create a
  Power Apps connector", "expose this API to Power Apps/Power Automate".
  For general fixes to an existing connector or its backing API, prefer
  the powerapps-developer agent; use this skill when standing up a new
  connector end to end.
---

# Power Platform Custom Connector Workflow

A custom connector's OpenAPI definition becomes a public contract the
moment any maker builds an app against it — this workflow treats it that
way from the first draft, not as an implementation detail to firm up
later.

## Step 1 — Confirm the underlying API is connector-ready

- The connector wraps a REST API — that API should already be (or be
  made) well-behaved: consistent status codes, clear structured error
  bodies, pagination on list endpoints, and stable resource-oriented
  routes. A connector layered on top of an inconsistent API just exposes
  that inconsistency to makers with an extra layer of indirection — fix
  the underlying API first if it isn't there yet (see
  aspnet-core-developer for that work).
- If no such API exists yet, this is itself a piece of work to plan via
  the new-feature workflow before returning to connector authoring.

## Step 2 — Choose the auth model

Match whatever the underlying API actually requires — don't invent a new
scheme in the API purely to simplify the connector:

- **API Key**: simplest; appropriate for server-to-server or
  low-sensitivity scenarios where a single (or per-environment) key is
  acceptable.
- **OAuth 2.0**: for anything requiring per-user identity/consent or
  finer-grained scopes — requires registering an OAuth app
  (client ID/secret, authorization/token endpoints) with the connector.
- **Basic / No auth (behind another gateway)**: rare for new work; only
  appropriate when another layer (a gateway, network boundary) already
  handles authentication and the connector genuinely doesn't need its
  own.
- Never hardcode the connector's registered client secret/API key
  anywhere in this repo — store it in the connector's own connection
  configuration (set up through the Power Platform admin/maker
  experience), never in source.

## Step 3 — Author the OpenAPI definition

- Write (or generate, then hand-review) a Swagger 2.0 definition
  (current custom-connector tooling requirement) describing every
  operation the connector exposes: path, verb, parameters, request/
  response schemas, and a clear `summary`/`description` per operation —
  makers browsing the connector rely on these descriptions to use it
  correctly without reading the underlying API's own docs.
- Give every operation a stable `operationId` — canvas app formulas and
  Power Automate flow definitions reference operations by this ID; a
  later rename orphans anything already built against it.
- Define request/response schemas precisely (types, required fields,
  enums where applicable) rather than loosely-typed free-form objects —
  this is what gives makers usable typed data in the Power Apps/Power
  Automate designer instead of an opaque blob.
- Keep the definition additive once anything depends on it: new
  operations and new optional parameters/fields are safe; changing an
  existing operation's required parameters, removing a field a maker's
  formula already references, or renaming an `operationId` are breaking
  changes — see Step 5.

## Step 4 — Consider Dataverse implications, if relevant

- If the connector's data ultimately needs to interoperate with
  Dataverse (e.g., a model-driven app or Power Automate flow will write
  results into Dataverse tables), design the connector's response shapes
  to map cleanly onto the target Dataverse table's columns/types rather
  than requiring awkward transformation logic in every flow that
  consumes it.
- If the connector is a *replacement* for direct Dataverse access (the
  data could live in Dataverse instead of behind a custom API), weigh
  that explicitly — Dataverse's built-in security model, auditing, and
  native Power Platform integration are often a better fit than a custom
  connector wrapping equivalent CRUD; don't default to "always build a
  connector" without considering whether Dataverse itself is the more
  appropriate backend (see powerapps-developer's canvas-vs-model-driven
  guidance).
- If the underlying API writes to Dataverse itself (via the Dataverse SDK
  or Web API), apply the same service-principal-plus-security-role and
  service-protection-limit considerations covered in powerapps-developer.

## Step 5 — Version deliberately

- Add new operations/parameters/response fields additively to the
  existing connector definition whenever possible.
- For a genuinely breaking change (removed/renamed operation, changed
  required parameters, changed response shape in an incompatible way),
  create a new connector (or a new versioned host/path) rather than
  mutating the existing one in place — existing canvas apps/flows keep
  working against the old connector while makers migrate deliberately to
  the new one.
- Document the connector's contract and any versioning decision in the
  project's existing documentation location.

## Step 6 — Test and validate

- Test the underlying API directly first (unit/integration tests per the
  project's detected framework) — the connector layer adds no logic of
  its own to test beyond the OpenAPI definition's accuracy.
- Validate the OpenAPI definition itself (schema validation, and ideally
  importing it into the Power Platform connector authoring experience to
  confirm it registers cleanly) before considering the connector done.
- If a sandbox Power Platform environment is available, exercise at
  least one operation from an actual canvas app or flow to confirm the
  end-to-end contract behaves as the definition promises — a definition
  that validates syntactically can still describe behavior the API
  doesn't actually deliver.

## Do
- Design the underlying API well before wrapping it in a connector.
- Give every operation a stable `operationId` and precise schemas.
- Version breaking changes as a new connector, not an in-place mutation.
- Consider whether Dataverse itself is the better backend before
  defaulting to a custom connector.

## Don't
- Don't hardcode connector auth secrets anywhere in source.
- Don't rename/remove an operation, parameter, or response field that
  existing apps/flows may already reference.
- Don't loop individual Dataverse calls if the connector's backing API
  performs bulk Dataverse writes.
- Don't call a connector done without validating the OpenAPI definition
  and, where possible, exercising it end to end.
