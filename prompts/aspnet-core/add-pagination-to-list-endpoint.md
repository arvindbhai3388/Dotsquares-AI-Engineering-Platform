# Add Pagination to a List Endpoint

**Category:** ASP.NET Core
**Use when:** a collection endpoint risks returning very large payloads.

## Prompt

Analyze the list endpoint I specify: the current query behind it (`IQueryable` composition, raw SQL, or in-memory filtering of an already-materialized collection), the realistic size of the underlying dataset today and its growth trend, whether consumers currently depend on receiving the full unpaginated array (a backward-compatibility concern), and whether any other endpoint in this codebase already implements pagination whose contract/shape I should match for consistency.

Propose the pagination approach before implementing: offset-based (`page`/`pageSize` or `skip`/`take` — simple, but has consistency issues with concurrent inserts/deletes shifting pages) versus keyset/cursor-based (an opaque or explicit cursor keyed on a stable sort column — more correct under concurrent writes, better performance on large tables, but a bigger change). Recommend one based on the dataset size and mutation frequency, and propose the response envelope shape (items plus metadata: total count if offset-based and cheap to compute, `nextCursor`/`hasMore` if keyset-based) matching existing conventions if any exist. Confirm default and maximum page size limits so a client can't request an unbounded page size.

Once approved, implement:
- Push the pagination down into the actual query (`Skip`/`Take` on `IQueryable`, or a `WHERE` clause on the cursor column for keyset) rather than materializing the full result set and paging in memory.
- Enforce a maximum page size server-side regardless of what the client requests.
- Ensure the sort order is deterministic (a stable `ORDER BY` including a unique tiebreaker column) — pagination over a non-deterministic sort produces duplicate/missing rows across pages.
- Validate query parameters (negative/zero page size, malformed cursor) and return a 400 rather than an exception.
- If this changes an existing endpoint's response shape from a bare array to an enveloped object, treat this as a breaking change and confirm with me how existing consumers should be migrated (new endpoint/version versus in-place change).

Write or update tests covering: first page, last page, empty result set, a page size exceeding the max being clamped or rejected per the agreed behavior, and (for keyset) correct behavior when items are inserted/deleted between page fetches. Confirm with me on the breaking-change handling before changing a live endpoint's response shape.
