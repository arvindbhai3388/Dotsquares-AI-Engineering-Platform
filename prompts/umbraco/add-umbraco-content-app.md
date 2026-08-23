# Add a Custom Content App Tab

**Category:** Umbraco CMS
**Use when:** Editors need a custom view/tool alongside the standard content properties (e.g., SEO preview, related content, workflow status).

## Prompt

I need to add a custom Content App -- an additional tab in the backoffice content-editing screen, alongside "Content" and "Info" -- to give editors a specialized tool (e.g., an SEO/meta preview panel, a "referenced by" list showing which other nodes link here, or a publish-readiness checklist). Locate any existing Content Apps already registered in this codebase (search for `IContentAppFactory` implementations or `package.manifest` contentApp entries) to match the established registration pattern and file layout under `~/App_Plugins/`.

Propose the plan before implementing:
1. Registration approach: a C# `IContentAppFactory` (for conditional visibility, e.g., only show this tab for certain Document Types or user permissions) versus a static `package.manifest` contentApp entry (simpler, always visible where configured).
2. The tab's condition logic -- which Document Type alias(es) or content Ids it should appear for, and whether visibility depends on the current user's permissions/group.
3. The client-side view: what data it needs from the current content node (fetched via the existing Content or Media management API, or a new lightweight backoffice-only API controller extending `UmbracoAuthorizedApiController` if custom data is required), and how it should behave for an unsaved/new (not-yet-persisted) content node where the node Id may not exist yet.
4. Any write-back behavior (does this tab only display information, or can editors change something from it?) and how that interacts with the standard Save/Publish buttons -- avoid silently saving data outside Umbraco's normal save/publish lifecycle, which can confuse editors about what "Save" actually commits.

Wait for my approval, then implement: the content app registration, the Angular/Web Component view and controller, and any backend API endpoint needed, secured with the same backoffice authorization as other custom API controllers in this codebase. Validate by confirming the tab appears only where intended, renders correctly for both existing and brand-new unsaved content, and does not interfere with normal Save/Publish/Unpublish actions on the node. Note any effect on editors' page-load time if the tab fetches data eagerly rather than on tab-activation.
