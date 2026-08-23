# Wire Up a Content Picker in a View

**Category:** Umbraco CMS
**Use when:** A template needs to reference another content node (e.g., a "Featured Page" or "Related Article" link).

## Prompt

I need to correctly wire up and render a Content Picker (or Multi-Node Tree Picker, if multiple references are needed) property so a template can link to or pull data from another content node. Start by locating the Document Type and confirming whether the property already exists or needs to be added, and check whether ModelsBuilder is in use (strongly-typed `IPublishedContent`-derived models) or whether the codebase accesses `IPublishedContent` properties dynamically/by alias.

Propose the approach before implementing:
1. If adding the property: Content Picker for a single reference, Multi-Node Tree Picker for multiple, with an appropriate "start node" restriction so editors cannot pick content from unrelated sections of the tree.
2. How the view will resolve the picked value: `.Value<IPublishedContent>("propertyAlias")` (single) or `.Value<IEnumerable<IPublishedContent>>("propertyAlias")` (multi), and how it will be strongly typed if ModelsBuilder is active.
3. Explicit handling for every edge case: property not set (null), picked node unpublished or trashed/deleted since being picked (Umbraco returns null or a stale reference depending on version -- verify against the installed Umbraco version), picked node in a different language/culture (unmapped variant), and picked node the current user's front-end context should not have access to (e.g., member-restricted content).
4. Whether the resolved content should render a link (`Url()`/`UrlSegment`), a card/teaser (title + image + excerpt pulled from the picked node's own properties), or be used for data lookups only.

After I approve, implement with defensive null checks throughout (never assume the picked node exists or is published), and avoid N+1 patterns if this picker is rendered inside a list/loop -- batch-resolve via `IPublishedContentQuery` or cache as appropriate. Verify rendering in the browser preview for: property empty, property pointing to a published node, and property pointing to an unpublished/deleted node, confirming no exceptions and a sensible fallback UI in each case.
