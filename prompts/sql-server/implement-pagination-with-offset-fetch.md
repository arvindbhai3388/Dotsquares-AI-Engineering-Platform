# Implement Efficient Server-Side Pagination

**Category:** SQL Server
**Use when:** An API or report needs paged results from a large table and current pagination (or lack of it) is slow or inconsistent.

## Prompt

Design and implement server-side pagination for the attached query/endpoint, choosing between `OFFSET ... FETCH NEXT` and keyset (seek-based) pagination based on the actual table size and access pattern rather than defaulting to one approach. First check the table's row count, whether the sort column(s) are indexed and unique (or can be made unique with a tiebreaker column such as the primary key), and how the UI/API consumes pages (jump-to-page-N requires OFFSET-style access; infinite-scroll/"next page" access is a good fit for keyset pagination).

If OFFSET/FETCH is appropriate (smaller tables, or a genuine need for arbitrary page-number jumps), ensure the query has an `ORDER BY` on an indexed column(s) with a deterministic tiebreaker (never paginate on a non-unique or unindexed sort key, since results can duplicate or skip rows between pages under concurrent writes), and confirm the plan uses an index seek for the ordering rather than sorting the full result set before applying OFFSET. Be explicit that OFFSET/FETCH cost grows with the offset value (SQL Server still traverses skipped rows), so warn if deep pagination (e.g., page 500 of a million-row table) is expected — that's a strong signal to use keyset pagination instead.

If keyset pagination is appropriate, implement it using a `WHERE (SortCol, TieBreakCol) > (@lastSortVal, @lastId)` (or the SQL Server-compatible row-value-comparison equivalent) pattern against an index that supports the sort order, so each page is a seek regardless of depth, and design the API contract to pass the last-seen key rather than a page number.

For either approach, use parameterized queries (never concatenate the page size/offset), and consider `OPTION (RECOMPILE)` if wildly different page sizes cause parameter-sniffing issues. Provide the exact query, the supporting index if one doesn't already exist, and the API/repository code change following this project's existing data-access pattern. Propose the change and wait for approval before running any new index creation against a production or production-like database.
