# Implement SharePoint List CRUD via Microsoft.Graph SDK

**Category:** SharePoint (Microsoft Graph)
**Use when:** An app needs to read/write SharePoint list items programmatically.

## Prompt

Implement full CRUD (create, read, update, delete) operations against a SharePoint list item collection using the `Microsoft.Graph` SDK's `GraphServiceClient`. Locate the existing service/repository layer pattern in this codebase first and follow it rather than inventing a new abstraction.

Requirements:
- Wrap access behind an interface (e.g., `ISharePointListService`) so the Graph SDK is not leaked into controllers/business logic, matching this repo's existing DI and service-layer conventions.
- For reads, use `graphClient.Sites[siteId].Lists[listId].Items.GetAsync()` with `$expand=fields` to pull field values, and handle pagination correctly (see the pagination edge case below) rather than assuming all results fit in one page.
- For create, build a `ListItem` with a `FieldValueSet` and POST it; for update, patch only the changed fields via `FieldValueSet` rather than replacing the entire fields collection; for delete, confirm the item exists first and handle a 404 gracefully instead of throwing.
- Map Graph's dynamic `AdditionalData` field values into strongly typed DTOs specific to the list's schema — do not pass raw dictionaries up through the service boundary.
- Handle Graph throttling (429/503) using this repo's existing retry mechanism if one exists, or flag that a Polly policy is needed (see the throttling-retry-policy-with-polly prompt) rather than adding an ad hoc retry loop here.
- Handle and translate `ODataError` / `ServiceException` into this app's existing exception/result conventions; never let raw Graph exceptions bubble to the API layer.
- Never hardcode site IDs, list IDs, tenant IDs, or credentials — pull them from configuration/options following existing patterns, and flag if a required config value looks like it should live in a restricted secrets file.
- Confirm least-privilege Graph permissions (`Sites.Selected` plus item-level grants where possible) are sufficient for these operations before assuming `Sites.ReadWrite.All` is needed.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the interface shape and DTO mapping first, wait for approval, then implement with unit tests covering success, not-found, validation, and throttled-retry paths.
