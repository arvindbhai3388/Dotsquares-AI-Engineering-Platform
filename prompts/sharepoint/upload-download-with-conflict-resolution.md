# Implement ETag-Based Conflict Detection for SharePoint File Upload/Download

**Category:** SharePoint (Microsoft Graph)
**Use when:** Multiple users/processes may modify the same file concurrently.

## Prompt

Implement upload and download of SharePoint files with ETag-based optimistic concurrency so that when two users or processes edit the same file concurrently, the second writer doesn't silently overwrite the first writer's changes.

Requirements:
- On download/read, capture and return the file's current `eTag` (from the `DriveItem.ETag` property) alongside the content to the caller, so any subsequent update can be conditioned on it.
- On upload/update, pass the previously captured `eTag` as an `If-Match` header on the update request (for simple uploads via `graphClient.Drives[driveId].Items[itemId].Content.PutAsync()`, or on the relevant call in an upload-session flow if updating an existing large file). If Graph responds with `412 Precondition Failed`, this means the file changed since it was last read — do not treat this as a generic error; surface it as a specific conflict result/exception distinct from other failures so calling code can react appropriately.
- Define and implement an explicit conflict-resolution strategy appropriate to the feature (ask me which applies if unclear): reject-and-notify (tell the user their change couldn't be saved because the file changed, let them reload and retry), last-writer-wins-with-warning (proceed but log/flag that an overwrite occurred), or merge-if-possible (only relevant for structured content, not typically for opaque file bytes). Do not silently pick last-writer-wins without flagging it, since that's the exact behavior conflict detection is meant to prevent.
- For list items (not just drive files), apply the same pattern using the item's `eTag` and `If-Match` on the PATCH request to `Items[itemId]`.
- Handle the case where the item was deleted between read and write (404 on the conditional update) as a distinct outcome from a version conflict (412).
- Ensure retries from the throttling policy do not re-send a stale `If-Match` value from a cached prior response — the conditional header must reflect the version this specific write attempt is based on.
- Never assume client-side timestamps are a reliable substitute for `eTag` comparison; SharePoint/Graph timestamps can have coarser resolution or clock-skew issues across distributed writers.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose which conflict-resolution strategy fits this feature first, then implement with tests covering a clean update, a 412 conflict on stale ETag, and a 404 from a concurrently deleted file.
