# Implement Cross-Site SharePoint Search via Microsoft Search API

**Category:** SharePoint (Microsoft Graph)
**Use when:** Users need to search across multiple lists/libraries/sites at once.

## Prompt

Users need to search across multiple SharePoint lists, document libraries, and possibly multiple sites at once. Implement this using the Microsoft Search API surfaced through Graph (`POST /search/query`) rather than writing manual per-list `$filter` queries fanned out across every list, which does not scale and misses SharePoint's relevance ranking and content indexing.

Requirements:
- Build the `SearchRequest` with `entityTypes` set appropriately (`listItem`, `driveItem`, or both) and a KQL (`query.queryString`) built from user input — treat user-supplied search text as untrusted, and escape/sanitize it so a user cannot inject KQL operators to search outside their intended scope or pull unintended fields.
- Scope results correctly: Microsoft Search results are already filtered to what the calling identity has permission to see (for delegated auth) — call out clearly in code comments whether this call uses delegated or app-only auth, since app-only search results do not carry that per-user filtering and must not be shown to end users without an equivalent authorization check in this app.
- Implement pagination using the `from` and `size` fields on repeated requests (Search API pagination differs from `@odata.nextLink` used elsewhere in Graph) and expose a clean paged result to callers.
- Request only the fields needed via `fields` in the request to avoid over-fetching, and map returned `resource` hit content into this app's existing search-result DTOs rather than passing raw Graph objects to the UI/API layer.
- Handle empty result sets, malformed queries, and throttling (429/503) using the existing retry policy.
- If results need to be aggregated with local application data (e.g., joining a SharePoint hit to an internal record), do that join after retrieving results, not by trying to push local IDs into the KQL query.
- Confirm required permissions (`Sites.Read.All` or narrower with `Sites.Selected`, plus delegated `User.Read` context if applicable) before implementing.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the KQL construction/sanitization approach and the delegated-vs-app-only decision first (this has real data-exposure implications), then implement with tests covering a normal query, an empty result, a query containing special KQL characters from user input, and paged retrieval.
