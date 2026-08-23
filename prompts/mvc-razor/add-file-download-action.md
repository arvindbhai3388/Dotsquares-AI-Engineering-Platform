# Add a Secure File Download Action

**Category:** ASP.NET MVC / Razor Pages
**Use when:** users need to download a generated or stored file (report, export, attachment).

## Prompt

Add a controller action that lets an authenticated/authorized user download a file (a generated report, an export, a stored attachment). Before implementing, confirm with me: where the file actually lives (generated on demand, stored on disk, stored in blob storage, or stored as bytes in the database), the expected content type, and -- critically -- how the requested file identifier maps to the actual file, since this is the most common place file-download actions become a path-traversal or IDOR vulnerability.

Implement authorization first: verify the current user is allowed to access this specific file/resource (object-level authorization, not just "is authenticated"), and return 403/404 (prefer 404 to avoid confirming a resource's existence to an unauthorized user, matching this project's existing convention if one exists) before touching the filesystem or storage. Never build a file path by concatenating a raw user-supplied filename/id directly onto a base directory -- resolve the identifier against a known record (e.g., a database row with a stored, server-controlled path) rather than trusting client input as a path component, and reject/normalize any input containing path traversal sequences if a filename must be derived from user input at all.

Stream the file rather than loading it fully into memory when it could be large -- use `FileStreamResult`/`PhysicalFileResult`/`return File(stream, contentType, downloadFileName)` (or this project's established pattern for this) so the response streams rather than buffering the entire file. Set `Content-Disposition` explicitly with a sanitized download filename (strip/encode characters that could break the header or enable header injection), and set the correct `Content-Type` rather than defaulting to `application/octet-stream` unless that's genuinely appropriate.

Ensure the file handle/stream is disposed correctly (the framework disposes the stream passed to `FileStreamResult` automatically -- don't dispose it yourself and double-dispose). Write tests covering: authorized download succeeds with correct headers, unauthorized/not-found returns the expected status, and a path-traversal-style identifier is rejected.
