# Add Bookmarks and Custom In-App Navigation

**Category:** Power BI
**Use when:** A report needs a guided, presentation-style navigation experience.

## Prompt

Add support for Power BI report bookmarks and wire up custom in-app navigation buttons (outside the default Power BI toolbar) that jump the embedded report between bookmarked states, for a guided/presentation-style navigation experience. This is primarily frontend work against the already-embedded report object, so confirm the existing embed integration (see this app's embed-with-token-refresh implementation) before adding to it, rather than re-embedding from scratch.

Implementation requirements:
- Confirm the target bookmarks already exist on the report (bookmarks are authored in Power BI Desktop's Bookmarks pane and published with the report) -- this task is about consuming existing bookmarks from application code, not generating them; if the bookmarks don't exist yet, tell me they need to be authored in Desktop first rather than trying to create them via the REST API's limited bookmark support.
- Retrieve the report's bookmark list via the `powerbi-client` JS SDK (`report.bookmarksManager.getBookmarks()`) once the report has finished loading (listen for the `loaded` event before calling this -- calling it too early returns an empty or stale list).
- Build custom navigation UI (buttons, a stepper, or a menu, matching this app's existing UI component conventions -- reuse existing button/nav components rather than introducing new styling patterns) that calls `report.bookmarksManager.apply(bookmarkName)` or `applyState(state)` for a given bookmark's name/state when clicked.
- Handle the "personal bookmarks" vs "report bookmarks" distinction if relevant -- report bookmarks (author-defined, used for guided navigation) are what this task needs; do not conflate with the SDK's personal-bookmark save feature unless that's explicitly also wanted.
- If bookmarks apply to only specific pages, ensure navigating to a bookmark also navigates to the correct report page first (`report.setPage()`) if the SDK doesn't do this implicitly for the target bookmark's page context.
- Keep navigation state in sync with the report: if the user manually changes the report's filters/page outside the custom nav, decide (and confirm with me) whether the custom nav should show an "unsaved/modified" indicator or simply not track drift.

Manually verify the navigation buttons correctly reproduce each bookmarked state (filters, slicers, visual visibility, drill state) as part of Validate, since bookmark application can silently fail to restore a specific visual's state if the bookmark was authored against a since-changed report layout.
