# Handle Graph API Pagination with @odata.nextLink

**Category:** SharePoint (Microsoft Graph)
**Use when:** A query can return more items than fit in a single Graph response page.

## Prompt

Review and correct (or implement from scratch) pagination handling for a Graph query against a SharePoint list or drive that can return more results than fit in a single page, using `@odata.nextLink` correctly rather than assuming a single response call returns everything.

Requirements:
- Use the Graph SDK's built-in page iterator (`PageIterator<T, TCollectionResponse>.CreatePageIterator(...)`) where practical instead of hand-rolling `nextLink` following — it already handles the follow-the-link loop, deduplication of the base request options, and works with the SDK's paging model correctly.
- Where a manual loop is more appropriate (e.g., custom per-page processing or when the iterator's callback model doesn't fit), follow `@odata.nextLink` exactly as returned by Graph — treat it as an opaque URL, do not attempt to parse or reconstruct `$skiptoken`/`$skip` values from it, since Graph's internal paging tokens are not guaranteed stable across versions.
- Accept and honor a `CancellationToken` through the entire paging loop so a caller can cancel a long-running multi-page fetch (e.g., a UI-triggered search should stop pulling pages if the user navigates away) rather than the loop running to completion regardless.
- Set an explicit, sensible `$top` page size for the initial request rather than relying on Graph's default, and be aware that Graph may still return fewer items than requested per page even before the final page — the loop must check for `nextLink` presence, not compare returned-count to requested-count, to decide whether more pages remain.
- Apply the existing (or newly agreed) Polly throttling policy per page request, since a long paging loop over a large list is exactly the scenario most likely to trigger 429 responses partway through.
- Avoid materializing the entire result set in memory for very large lists/drives if the consumer can process items as a stream (`IAsyncEnumerable<T>` yielded per item as pages arrive) — check whether the calling code actually needs the full list at once before defaulting to a `List<T>` accumulator.
- Never leak the raw Graph page/response objects past the service boundary; project into this app's DTOs as each page is processed.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose whether to use `PageIterator` or a manual loop and the streaming-vs-buffered decision first, then implement with tests covering multi-page results, cancellation mid-loop, and a throttled page triggering a retry.
