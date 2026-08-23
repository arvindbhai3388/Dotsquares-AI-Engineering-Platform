# Decide Between AsSplitQuery and Single Query for Multi-Include Queries

**Category:** Entity Framework Core
**Use when:** A query with several Include() calls is producing a cartesian-explosion-sized result set.

## Prompt

The query in [name the method/repository] loads [entity] with multiple `.Include()` calls on collection navigations (e.g., `.Include(o => o.OrderLines).Include(o => o.Payments)`), and I'm concerned about (or have measured) a cartesian explosion inflating the result set size and query time. Walk through analyze -> propose -> approve -> implement -> test -> review before changing anything.

Analyze:
1. Read the query and count how many collection (one-to-many/many-to-many) navigations are being included in the same query versus how many are reference (one-to-one/many-to-one) navigations -- only collection-to-collection combinations cause the multiplicative row explosion; a single collection include does not.
2. If possible, capture the generated SQL and actual row count returned versus the logical row count expected, to quantify the inflation.
3. Check the current EF Core version's default behavior (single query by default since EF Core 5+ unless configured otherwise) and confirm whether `UseQuerySplittingBehavior` is set globally or needs to be set per-query.

Propose a decision with tradeoffs, not just a default:
- `AsSplitQuery()`: issues one SQL query per included collection (avoids row duplication and reduces payload size) but loses transactional consistency between the queries (a concurrent write between the split queries could produce a slightly inconsistent snapshot) and adds round trips -- recommend it when multiple collections are included and result set inflation is significant.
- Single query (default): one round trip, consistent snapshot, but the duplicated parent columns across every child row can be expensive over the wire for wide parent entities with large child collections -- recommend it when only one collection is included or data volume is small.
- Consider whether the real fix is a projection (`.Select()` into a DTO) instead of either, avoiding the tradeoff entirely by not hydrating full entity graphs.

Wait for my approval on the chosen approach before editing the query.

Implement the change, keeping it scoped to the specific query/repository method.

Test: assert correctness of returned data (no missing/duplicated child rows) and, if feasible, add a benchmark or row-count assertion comparing before/after.

Review: confirm the choice is documented with a short comment explaining why, since this tradeoff is non-obvious to future readers.
