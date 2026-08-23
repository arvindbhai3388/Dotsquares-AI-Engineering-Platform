# Build a Complex SharePoint List Filter (Graph OData with CAML/CSOM Fallback)

**Category:** SharePoint (Microsoft Graph)
**Use when:** Default list queries don't support the required filtering/sorting logic.

## Prompt

I need a complex filter/sort over a large SharePoint list that the default Graph query doesn't seem to support cleanly. First, try to express the full query using Microsoft Graph OData query parameters against `graphClient.Sites[siteId].Lists[listId].Items.GetAsync()` — `$filter` on indexed/supported fields, `$orderby`, `$top`/`$skip` or `$skiptoken` for paging, and `$expand=fields($select=...)` to project only needed columns. Only fall back to CAML via CSOM/PnP if the specific filter genuinely cannot be expressed in Graph OData (e.g., certain lookup-field joins, some full-text-in-multiline-field scenarios, or filters on more fields than SharePoint's list-view threshold allows without an indexed column).

Requirements:
- Explain, for the specific filter I describe, exactly why Graph OData can or cannot express it before writing any CAML — do not reach for CAML/CSOM by default since it adds a second SDK/auth dependency (CSOM typically needs its own SharePoint Online credentials/context separate from the Graph app registration) that this codebase may not already have.
- If Graph OData suffices, implement it with parameterized query construction — never string-concatenate user-supplied filter values directly into the `$filter` string, to avoid OData injection; use the Graph SDK's query parameter builders or properly escape/quote values.
- If CAML/CSOM is genuinely required, isolate it behind the same repository/service interface used by the rest of the list-access code (see the list-crud-via-graph-sdk prompt's `ISharePointListService` pattern if it exists) so callers don't need to know which underlying technology answered the query, and clearly document why this one query needed CSOM.
- Respect SharePoint's list-view threshold (5000 items by default) — a filter on a non-indexed column against a large list will throw; detect this case and either require an indexed column or reduce scope, rather than letting it fail unexplained in production.
- Handle pagination on the result set regardless of which technology answers the query.
- Add appropriate indexed columns to the SharePoint list only after getting explicit approval, since that's a change to shared SharePoint list configuration, not just this app's code.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the OData expression (or the specific justification for CSOM/CAML) first, then implement with tests covering the filter against representative data, empty results, and the list-view-threshold error path.
