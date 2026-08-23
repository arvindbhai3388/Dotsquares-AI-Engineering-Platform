# Power Apps Integration

Guidance for integrating custom .NET backends with Power Apps and the wider Power Platform, covering custom connectors, Dataverse, and the canvas-app/model-driven-app backend decision.

## Custom connector design

A custom connector is how a Power App (canvas or model-driven, via a Power Automate flow) calls into a bespoke REST API that isn't already covered by a standard/premium Microsoft connector.

- Design the underlying REST API **connector-first** — a clean OpenAPI (Swagger) definition with clear operation IDs, required/optional parameters, and realistic example responses, since the custom connector wizard imports directly from an OpenAPI document (or a Postman collection) and low-quality/ambiguous schema definitions produce a clumsy, hard-to-use connector surface inside Power Apps' visual designer.
- Keep operations **coarse-grained and task-oriented** rather than exposing a raw CRUD mirror of internal entities — a maker building a canvas app benefits far more from `GetCustomerOrderSummary(customerId)` returning exactly the shape a screen needs than from generic `GetOrders`/`GetOrderItems`/`GetCustomer` calls the maker has to compose and join themselves inside Power Fx formulas, which is a poor substitute for a proper join done server-side.
- **Authentication**: support OAuth 2.0 (Azure AD/Entra ID) as the default for connectors used inside a Dotsquares client's own tenant — this lets the connector participate in the tenant's existing conditional access/MFA policies and avoids a separate credential to manage. API-key auth is acceptable for simpler, lower-sensitivity integrations but must never use a single shared key embedded in a canvas app's own formulas (visible to any maker who can open Advanced Settings) — route it through the connector's connection configuration instead, where the key is stored by the Power Platform connection object, not the app itself.
- Version custom connectors deliberately (a new connector or a new API version path) rather than changing an in-use operation's request/response shape in place — every canvas/model-driven app and every Power Automate flow already built against the old shape breaks silently the moment the connector's contract changes, and there is no compiler to catch it the way there would be for a strongly-typed client.
- Define explicit, useful error responses in the OpenAPI definition (4xx/5xx with a descriptive body) — Power Apps surfaces connector errors to makers with limited ability to inspect raw HTTP details, so a generic `500` with no body leaves a maker with nothing actionable to build error handling around in their app's `IfError`/`OnError` formulas.
- Respect Power Platform's own request size and timeout constraints (a custom connector call has a request timeout in the range of 100 seconds for its underlying HTTP call and stricter limits inside Power Automate) — a long-running backend operation should be modeled as fire-and-start-then-poll-for-status rather than a single long synchronous call the connector will time out waiting for.

## Dataverse integration patterns

Dataverse (the Power Platform's own structured data store, underlying model-driven apps and Dynamics 365) is a common integration target from a Dotsquares .NET backend:

- Use the **Dataverse Web API** (an OData v4 REST API) from server-side .NET code for most integration scenarios — authenticate via Azure AD app registration (client credentials flow, an "application user" configured in Dataverse's own security model with an assigned security role) for unattended service-to-service calls, the same app-only pattern used for [SharePoint](SharePoint-Integration.md) and [Power BI](PowerBI-Integration.md).
- The **Dataverse SDK for .NET** (`Microsoft.PowerPlatform.Dataverse.Client`, the modern replacement for the older `Microsoft.Xrm.Sdk`/`CrmServiceClient`) is preferable when the integration needs strongly-typed early-bound entity classes generated from the Dataverse schema, or needs SDK-level features (bulk operations via `ExecuteMultipleRequest`, transactional batches) that are more awkward to construct by hand against the raw Web API.
- Model integration as **event-driven where possible**: Dataverse supports webhooks and Azure Service Bus/Event Grid-backed plugins/flows that fire on record create/update/delete, which is generally a better architectural fit for keeping an external system in sync than a .NET service polling Dataverse on a timer — reserve polling for cases where the external system genuinely cannot receive inbound calls (e.g., it sits behind a firewall with no reachable endpoint).
- Respect Dataverse's own **API request limits** (per-user, per-24-hour-period entitlements that vary by licensing) — a bulk backend job hitting Dataverse in a tight loop can exhaust the service account's daily API allocation; use `ExecuteMultipleRequest`/batch endpoints for bulk writes, and consider off-peak scheduling for large sync jobs.
- Map Dataverse's security model (business units, security roles, field-level security, and table/row-level sharing) explicitly against what the integrating service account actually needs — an "application user" in Dataverse defaults to only what its assigned security role grants, so plan the role rather than reaching for the System Administrator role out of convenience.

## Canvas vs. model-driven — backend considerations

| | Canvas apps | Model-driven apps |
|---|---|---|
| Primary data source | Flexible — Dataverse, SQL Server, SharePoint, a custom connector, or a mix, chosen per screen/control | Dataverse only — model-driven apps are generated from a Dataverse schema (tables, forms, views, business rules) |
| Backend integration surface | Custom connectors, standard connectors, direct Dataverse connection — the backend is whatever the maker wires up per data source | Primarily Dataverse plugins (server-side C# assemblies triggered on record events) and Power Automate flows triggered from Dataverse events |
| Where custom .NET logic runs | Outside the app entirely — a custom connector calling an externally hosted API, or a Power Automate flow's custom connector/Azure Function step | Often *inside* Dataverse itself as a registered plug-in assembly (sandboxed C#, synchronous or async) running on the Dataverse platform, in addition to any external API integration |
| Best fit | Task-specific, UI-flexible apps (a field inspection app, a custom approval form) where the data model doesn't need to be a full Dataverse schema | Data-model-centric line-of-business apps that benefit from Dataverse's built-in relationships, business process flows, and security model out of the box |
| .NET developer's typical role | Building/maintaining the custom connector's backend API; canvas app formulas themselves are Power Fx, built by makers, not .NET code | Writing Dataverse plugin assemblies (early-bound `Microsoft.Xrm.Sdk.Plugin.IPlugin` implementations) and/or the external API a model-driven app's flow calls out to |

- A Dataverse **plugin** is server-side C# that runs synchronously (or asynchronously) inside the Dataverse platform's own execution pipeline on record events (pre-validation, pre-operation, post-operation stages) — this is the model-driven-app-adjacent equivalent of a database trigger, and should follow the same restraint as a DB trigger: keep it fast, keep it focused on data integrity concerns local to that entity, and push anything slow or cross-system (an external API call, a long computation) to an asynchronous plugin step or a Power Automate flow instead of a synchronous pre-operation step that blocks the user's save.
- When a feature could be built as either a canvas app calling a custom connector, or a model-driven app plus a Dataverse plugin, the deciding factor is usually the data model: if the data is naturally relational and already fits (or should fit) Dataverse's own table model, model-driven is typically the better long-term fit; if the app is a lightweight, purpose-built tool over data that lives elsewhere (or across several disparate sources), canvas plus a custom connector is the better fit. Raise this as an explicit architectural decision in the [Propose](AI-Workflow-Discipline.md) step rather than defaulting either way.

## Related pages

- [SharePoint Integration](SharePoint-Integration.md) — the analogous Graph/app-only-auth pattern.
- [Security Guidelines](../docs/Security-Guidelines.md) — least-privilege scopes for Power Platform service accounts.
- [Architecture Overview](Architecture-Overview.md)
