# Walk Through SharePoint App Registration and Least-Privilege Permissions

**Category:** SharePoint (Microsoft Graph)
**Use when:** Starting a new SharePoint/Graph integration from scratch.

## Prompt

We are starting a brand-new SharePoint integration in this .NET app using the Microsoft Graph SDK. Before writing any integration code, analyze the feature requirements I describe (which sites, lists, or drives need to be read or written, and whether the code runs as a background service or on behalf of a signed-in user) and propose the Azure AD (Entra ID) app registration setup needed to support it.

Specifically:
1. Recommend whether this should be a single-tenant or multi-tenant app registration, and whether we need client credentials (app-only) or delegated permissions, based on the scenario I give you.
2. List the exact Microsoft Graph API permissions required, favoring the narrowest scope that satisfies the use case (e.g., `Sites.Selected` over `Sites.ReadWrite.All`, `Files.Read` over `Files.ReadWrite.All`) and explain the tradeoffs of each option.
3. If `Sites.Selected` is viable, describe the additional step of granting the app access to specific sites via the Graph `/sites/{site-id}/permissions` endpoint, since this is often forgotten.
4. Document the exact steps to register the app in Entra ID, create a client secret or certificate, and configure redirect URIs if delegated auth is used. Do NOT put any actual secret, tenant ID, or client ID values in code or config that gets committed — remind me to store them in a secret manager or environment-specific configuration that is excluded from source control.
5. Show how the resulting values (tenant ID, client ID, client secret/certificate thumbprint) should be wired into `Microsoft.Graph` `GraphServiceClient` construction via configuration/options binding and DI, not hardcoded.
6. Note admin consent requirements and how to communicate them to whoever manages the tenant.

Follow the analyze -> propose -> approve -> implement workflow: give me the permission list and registration plan first and wait for my approval before generating any registration or bootstrap code, since this has real security and tenant-wide implications.
