# Implement Large File Upload to SharePoint via Graph Upload Session

**Category:** SharePoint (Microsoft Graph)
**Use when:** Uploading files that exceed the simple upload size limit (4MB).

## Prompt

Implement large file upload to a SharePoint document library using a Microsoft Graph upload session, since simple PUT uploads only work reliably below 4MB. Use the `Microsoft.Graph` SDK's `LargeFileUploadTask` against a session created via `graphClient.Drives[driveId].Items[parentId].ItemWithPath(fileName).CreateUploadSession.PostAsync()`.

Requirements:
- Chunk the upload in multiples of 320 KiB (Graph's required chunk size alignment) and stream the file rather than loading the whole file into memory — this matters for files in the hundreds of MB to multi-GB range.
- Use `LargeFileUploadTask.UploadAsync()` with an `IProgress<long>` callback so calling code (e.g., a background worker or an MVC action) can report upload progress, and make this optional so it doesn't force a UI dependency into the service layer.
- Implement resumability: if the upload session's expiration (`expirationDateTime`) has not passed, support resuming an interrupted upload by querying the session URL for already-uploaded ranges instead of restarting from byte 0. Persist the upload session URL somewhere durable (matching this app's existing persistence pattern) if resumability needs to survive a process restart.
- Handle transient failures (network errors, 5xx, 429 throttling with `Retry-After`) with retry around individual chunk uploads, not just the outer call — a single chunk failure should not force restarting the entire upload.
- Validate file name and path for SharePoint's illegal character/length rules before starting the session, and surface a clear validation error rather than letting Graph reject it mid-upload.
- Clean up (`DELETE`) the upload session if the operation is cancelled or ultimately fails, to avoid leaving orphaned sessions in the target library.
- Confirm the app registration has adequate write permission (`Files.ReadWrite` scoped appropriately) for the target drive before implementing.
- Never hardcode drive IDs, folder paths, or credentials; source them from configuration.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the chunking/resumability design and where session state is persisted first, then implement with tests covering successful multi-chunk upload, resume-after-interruption, and permanent-failure cleanup paths (mock the Graph client, do not hit a live tenant in unit tests).
