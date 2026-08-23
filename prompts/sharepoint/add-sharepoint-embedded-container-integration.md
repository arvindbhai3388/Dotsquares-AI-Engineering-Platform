# Integrate with SharePoint Embedded Containers for App-Specific Storage

**Category:** SharePoint (Microsoft Graph)
**Use when:** Building a line-of-business app that needs its own document storage backed by SharePoint infrastructure without a visible SharePoint site.

## Prompt

This app needs its own isolated, app-specific document storage (per-tenant or per-customer) backed by SharePoint's storage and compliance infrastructure, but without provisioning a visible, browsable SharePoint site for each customer. Implement this using SharePoint Embedded containers via Microsoft Graph.

Requirements:
- Explain the SharePoint Embedded model to confirm it's the right fit before implementing: a Container Type must first be registered (this typically requires a one-time setup in the SharePoint Embedded admin/PowerShell tooling, done by whoever owns the tenant, not purely through app code) and associated with this app's Azure AD app registration; only after that can the app create and manage containers via Graph.
- Use `POST /storage/fileStorage/containers` to create a new container per logical unit (e.g., per customer/tenant/project) rather than reusing a single shared container for isolation-sensitive data, and store the returned container ID in this app's own data model, associated with the owning entity.
- Once a container exists, perform file operations against it the same way as a regular drive (`graphClient.Storage.FileStorage.Containers[containerId].Drive...`) — reuse the existing list/upload/download/permission logic already built for regular SharePoint drives (see the large-file-upload-via-graph and manage-sharepoint-permissions-via-graph prompts) rather than duplicating that logic for containers.
- Use container-scoped Graph permissions (`FileStorageContainer.Selected` plus per-container role assignment via `POST /storage/fileStorage/containers/{id}/permissions`) rather than tenant-wide file permissions, since the entire point of containers is per-customer isolation — do not grant broader `Files.ReadWrite.All`-style access as a shortcut.
- Implement container lifecycle handling: containers have states (e.g., active, inactive) — do not assume a container ID captured earlier is always usable; check state or handle the relevant error before operating on it, and implement deletion/archival according to this app's data-retention requirements when the owning entity (customer/project) is removed.
- Never hardcode the Container Type ID or any container IDs; source them from configuration/the app's own data store.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: confirm the Container Type registration prerequisite is in place (or flag it as a blocking dependency on tenant admin action) and propose the container-to-entity mapping first, then implement with tests covering container creation, file operations against a container drive, and container-scoped permission assignment (mocked Graph calls).
