# Implement Delta Query Sync for a SharePoint List or Drive

**Category:** SharePoint (Microsoft Graph)
**Use when:** An integration needs to keep an external system in sync without re-pulling everything each time.

## Prompt

Implement delta query support so this integration only processes items that changed since the last successful sync, instead of re-fetching an entire SharePoint list or drive on every run. Use the Graph SDK's delta endpoints — `graphClient.Sites[siteId].Lists[listId].Items.Delta.GetAsDeltaGetResponseAsync()` for a list, or `graphClient.Drives[driveId].Root.Delta.GetAsDeltaGetResponseAsync()` for a document library.

Requirements:
- Persist the `@odata.deltaLink` returned at the end of each successful sync cycle (in this app's existing storage — SQL table, not a flat file, unless an existing pattern says otherwise) keyed per list/drive so the next run resumes from that point rather than doing a full sync.
- On first run (no stored delta link), perform an initial full sync and capture the delta link at the end; document this bootstrap behavior clearly in the code.
- Walk `@odata.nextLink` pages fully before treating the final page's `@odata.deltaLink` as the new checkpoint — a delta response can span multiple pages, and stopping early will corrupt the checkpoint.
- Handle deleted items: Graph represents deletions as items with a `deleted` facet (`{"deleted": {"state": "deleted"}}`) — detect this and propagate it as a delete/tombstone to the downstream system rather than trying to map it as an update.
- Handle the case where Graph invalidates the delta link (HTTP 410 Gone, "resync required") by falling back to a full resync and logging that this happened, since it usually means the token expired or the list schema changed materially.
- Make the sync idempotent: re-processing the same delta page twice (e.g., after a crash before the new delta link was persisted) must not create duplicate downstream records — key writes by the SharePoint item ID plus its `eTag`/last-modified value.
- Respect throttling (429/503) with the existing/agreed Polly retry policy during delta paging.
- Never hardcode site/list/drive IDs or credentials.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the delta-link storage schema and idempotency key first, then implement with tests covering first-run bootstrap, incremental sync with adds/updates/deletes, multi-page delta responses, and the 410-Gone resync fallback.
