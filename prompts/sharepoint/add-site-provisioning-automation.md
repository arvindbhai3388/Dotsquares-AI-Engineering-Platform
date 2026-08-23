# Automate SharePoint Site Provisioning from an App Event

**Category:** SharePoint (Microsoft Graph)
**Use when:** Manual SharePoint site setup per new client/project needs to be automated.

## Prompt

When a new project/client record is created in this application, automate provisioning of the corresponding SharePoint site (or document library) from a standard template, replacing the current manual setup process.

Requirements:
- Prefer Graph's site creation and template application where it covers the need: `POST /sites/{parent-site}/sites` or Graph's group-backed site creation (creating a Microsoft 365 Group provisions a linked team site), plus applying a site design/site script via `POST /sites/{site-id}/applySiteDesign` for structural elements (lists, columns, navigation). Only reach for PnP provisioning (PnP.Framework/PnP PowerShell templates, typically via a PnP `.pnp` template applied through PnP.Core SDK or a PnP provisioning engine) if the required setup genuinely exceeds what Graph site designs support — call out specifically what part needs PnP and why before adding that dependency, since it's a heavier dependency than staying on Graph alone.
- Trigger provisioning from the actual application event (e.g., the project/client creation workflow) via an async job (background worker/queue, matching this app's existing async patterns) rather than blocking the create request on a site-provisioning call that can take noticeably long and may transiently fail.
- Make the job idempotent: if provisioning partially fails (e.g., site created but site design application fails) and the job retries, it must detect the existing site and resume/complete remaining steps rather than attempting to create a duplicate site or erroring on "already exists" in a way that blocks the record permanently.
- Store the resulting site ID/URL back on the owning project/client record in this app's database as soon as it's known, so later Graph operations (list access, document upload) for that project have a stable reference rather than re-deriving the site by name/URL each time.
- Apply naming and URL conventions consistently and validate the desired site name/URL against SharePoint's naming restrictions before attempting creation, surfacing a clear error rather than a raw Graph 400.
- Set up default permissions on the new site according to the project's access model (e.g., project team members added as members, using the permission-management pattern from manage-sharepoint-permissions-via-graph) as part of the same provisioning flow, not as a manual follow-up step.
- Confirm the app registration has adequate site-creation permissions (`Sites.Manage.All` or the narrowest option that supports site creation) and that this was an explicitly approved scope, since site creation is a higher-privilege operation than read/write on existing sites.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the Graph-vs-PnP split and the idempotent provisioning-state design first, then implement with tests covering successful provisioning, resuming a partially-provisioned site, and a naming-validation rejection.
