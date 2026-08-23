# Add a Document Type with Properties and View

**Category:** Umbraco CMS
**Use when:** Modeling a new content type requested by a client.

## Prompt

I need to add a new Document Type to this Umbraco site and its matching front-end template. Follow the analyze -> propose -> approve -> implement -> test -> review workflow: first locate how existing, similar Document Types are structured (composition usage, property editor UI choices, allowed child types, template assignment) so the new one matches established conventions rather than inventing a new pattern.

Then propose a plan covering:
1. The Document Type alias, name, icon, and whether it should be an "Element Type" (for Block List/Nested Content use) or a full page type with a template.
2. Properties needed, each with the correct built-in property editor (Textstring, Richtext Editor, Content Picker, Media Picker, Checkbox, Dropdown, Block List, etc.) and appropriate Data Type configuration (do not create a new Data Type if an existing one with matching config already fits).
3. Compositions to reuse (e.g., an existing "SEO" or "Navigation" composition) instead of duplicating properties.
4. Allowed parent/child relationships in the content tree, and sort order considerations.
5. The Razor view/partial that renders it, using strongly-typed models (ModelsBuilder if this project uses it -- check for `~/App_Data/Models` or `Umbraco.ModelsBuilder` config before assuming) or `IPublishedContent` directly otherwise.

Wait for my explicit approval of the plan before creating anything.

On implementation: create the Document Type definition (via code-first `.uda` files if this project manages content types as code, or clearly state that backoffice creation is required, since Document Types are not always creatable purely via C#). Handle null/unpublished property values defensively in the view. Confirm the view enters the correct `~/Views/` folder matching the alias, and add it to any relevant navigation/sitemap logic. Flag if this changes the published content cache schema in a way that requires a republish of existing content, and check for backoffice UX impact (tab layout, property grouping, mandatory fields) before finishing. After implementation, tell me what manual backoffice steps remain if any part of the Document Type couldn't be created in code.
