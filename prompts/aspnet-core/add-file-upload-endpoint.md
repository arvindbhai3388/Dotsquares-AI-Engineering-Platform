# Add a Secure File Upload Endpoint

**Category:** ASP.NET Core
**Use when:** an API needs to accept user-uploaded files safely.

## Prompt

Analyze the requirements: expected file types, maximum size, where uploaded files ultimately get stored (local disk, blob storage, database), and whether any existing upload endpoint in the codebase already establishes conventions (size limits, storage service abstraction, virus-scanning integration) that this should match rather than duplicate.

Propose the design before implementing: the binding approach (`IFormFile`/`IFormFileCollection` for standard form uploads, or streaming via `Request.Body`/`MultipartReader` for very large files to avoid buffering the whole file in memory), the maximum allowed size and where it's enforced (`RequestSizeLimit`/`IISServerOptions`/`Kestrel` `MaxRequestBodySize` in addition to application-level checks — a client-controllable `Content-Length` header alone isn't a reliable limit), the file type validation strategy (never trust the client-supplied `Content-Type` header or file extension alone — validate actual file signature/magic bytes for the content types being accepted), the storage destination and naming scheme (generate a new server-side identifier for the stored filename; never use the client-supplied filename directly for the storage path, to prevent path traversal), and whether malware scanning is required before the file is made available for download.

Once approved, implement:
- Enforce size limits at both the server/Kestrel level and in application code, returning a clear 400/413 on violation rather than letting a huge upload exhaust memory.
- Validate content type via actual byte inspection where feasible for the accepted formats, not just extension/header trusting.
- Sanitize or discard the client-supplied filename; generate a safe server-side name/path, and store the original filename only as metadata if needed for display.
- Stream the file directly to its destination rather than loading the entire contents into a `byte[]`/`MemoryStream` for large files.
- Apply authorization checks before accepting the upload, and rate-limit or otherwise bound upload frequency if abuse is a concern.
- Never execute or interpret uploaded file content server-side.

Write or update tests covering: a valid upload within limits, a file exceeding the size limit, a disallowed file type (including one with a spoofed extension/Content-Type), and a filename crafted for path traversal (`../../etc/passwd`-style) being safely neutralized. Confirm with me before wiring any auto-scanning/quarantine bypass or storage location changes for an existing live upload path.
