# Add Authentication to a Custom Connector Definition

**Category:** Power Apps / Power Platform
**Use when:** The target API the connector calls requires authenticated access, not anonymous.

## Prompt

Add authentication configuration to the custom connector OpenAPI definition I point you to (or that we just created), matching the auth scheme the target .NET API actually enforces. First inspect the API's `Startup`/`Program.cs`/auth middleware to confirm whether it uses API key headers, OAuth2 (client credentials or authorization code), or Azure AD (Entra ID) bearer tokens -- do not assume; ask if the code doesn't make it obvious, and never open or quote the actual secret values from `appsettings*.json` even if they're referenced there.

Depending on the confirmed scheme, add the correct `securityDefinitions` block:
- **API key**: `type: apiKey`, correct `in` (header vs. query), and `name` matching the exact header/parameter the API expects; note in `x-ms-connector-metadata` that the key will be entered per-connection by whoever creates the connection in each environment.
- **OAuth2 client credentials**: `type: oauth2`, `flow: application`, the correct `tokenUrl` (Azure AD v2 token endpoint or the API's own), `scopes`, and confirm whether Power Platform's connector needs a registered Azure AD app registration with a client ID/secret -- explain that the client secret must be supplied when the connection is created in Power Apps/Automate, never hardcoded in the swagger file itself.
- **Azure AD bearer (Entra ID)**: use the `type: oauth2`, `flow: accessCode` pattern with the tenant-specific authorize/token URLs, and flag that the connector's Azure AD app registration needs the API's app ID URI added as an exposed scope, plus admin consent granted in each target environment (Dev/Test/Prod) -- this is a manual Azure AD step, not something achievable purely by editing the swagger file.
- For any scheme, add a `default` value hint or description text so whoever creates the connection in Power Apps/Power Automate understands what to enter, without exposing the real value.

Explicitly call out that per-environment ALM means each environment's connection is configured separately (the connector definition travels with the managed solution, but the actual secret/token is entered fresh in each environment) -- this ties into the environment-variable/connection-reference pattern used for other connectors here. Propose the diff to the swagger file, wait for approval, then apply it, and remind me which manual portal/Azure AD steps still need to happen outside of code.
