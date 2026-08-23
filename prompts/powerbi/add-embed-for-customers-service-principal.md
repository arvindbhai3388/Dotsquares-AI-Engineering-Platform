# Implement Embed-for-Your-Customers with a Service Principal

**Category:** Power BI
**Use when:** Building a multi-tenant SaaS app that embeds reports for external customers.

## Prompt

Implement the full "embed for your customers" pattern in this .NET application so external customers (who have no Power BI license or AAD account in our tenant) can view embedded reports, using a service principal end-to-end -- never the deprecated master user (username/password) authentication flow, which Microsoft no longer recommends and which cannot scale past a single set of hardcoded credentials.

Work through analyze -> propose -> approve -> implement -> test -> review. In the analysis step, confirm: the app registration exists with the required API permissions (Power BI Service application permissions, admin-consented), the service principal is added as a member of the target workspace(s) (or workspaces are assigned via a dedicated security group per the "Power BI admin portal -> Developer settings" configuration), and the workspace sits on a Premium/Embedded capacity (service-principal embedding requires a paid capacity, not the free per-user Pro license model).

Implementation scope:
- **Auth:** Acquire an AAD app-only token via MSAL's client-credentials flow (`AcquireTokenForClient`) using the app registration's client ID/secret or, preferably, a certificate -- read all identifiers from configuration, never hardcode them, and if using a certificate, load it from the platform's certificate store or key vault rather than a file checked into source.
- **Multi-tenancy isolation:** Map each external customer/tenant to the correct workspace and dataset (one workspace per customer, or shared workspace with RLS per customer -- confirm which multi-tenancy model this app uses before implementing, since the embed-token and RLS-identity logic differs materially between them).
- **Embed token generation:** Generate the embed token app-only (no `EffectiveIdentity` needed if using one-workspace-per-customer isolation; use `EffectiveIdentity` with RLS roles if using the shared-workspace-with-RLS model -- see the dedicated RLS prompt for that half).
- **Capacity awareness:** Handle the case where a workspace's capacity is paused or at capacity limits gracefully, surfacing a clear error rather than a generic 500.
- **Security:** Never expose the service principal's credentials, raw AAD tokens, or workspace/tenant IDs of other customers to the browser -- only the short-lived embed token and embed URL for the specific report the requesting customer is authorized to see.

Write tests covering per-customer workspace/dataset resolution and embed-token scoping, ensuring a request for customer A can never resolve to customer B's workspace even with a manipulated request parameter.
