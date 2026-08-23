# Write an OpenAPI Definition for a Custom Connector

**Category:** Power Apps / Power Platform
**Use when:** A Power Platform app/flow needs to call an internal API not covered by a standard connector.

## Prompt

Analyze the target .NET Web API controller(s) I point you to and produce a Power Platform custom connector OpenAPI 2.0 (Swagger) definition that exposes it to Power Automate and Power Apps. Before writing anything, locate the controller, its route attributes, request/response DTOs, and any existing Swashbuckle/Swagger generation already configured in the project, and tell me what you found (Understand -> Locate).

Requirements for the definition:
- Use `swagger: "2.0"` (Power Platform custom connectors do not support OpenAPI 3.0 natively as of this writing) unless the project already targets the newer OpenAPI 3 support explicitly.
- Set `host`, `basePath`, and `schemes: ["https"]` correctly, and add `x-ms-connector-metadata` values for capabilities like "Website", "Privacy policy", "Categories".
- For each operation, include `operationId` (short, PascalCase, human-readable in flow designer), `summary`, `description`, and `x-ms-visibility` where an operation or parameter should be advanced/internal.
- Map every route parameter, query parameter, header, and request body property to a `parameters` entry with accurate `type`, `format`, `required`, and `enum` values pulled from the actual DTOs -- do not invent fields.
- Define `responses` for at least 200/201, 400, and 401/403, each with a `schema` referencing a `definitions` entry generated from the real response DTO.
- Add a `securityDefinitions` block matching the API's actual auth (API key header, OAuth2 client credentials, or Azure AD) -- ask me which one applies if it isn't obvious from the code, rather than guessing.
- Call out any endpoint that isn't a good fit for a flow/app trigger (long-running, streaming, or file-upload endpoints need special handling) and propose an alternative.

Follow the analyze -> propose -> approve -> implement workflow: show me the parameter/schema mapping and any assumptions before finalizing the YAML/JSON file, and wait for my approval before writing it to disk. Save the definition under a clearly named file (e.g. `<ApiName>-connector.swagger.json`) rather than modifying existing project files.
