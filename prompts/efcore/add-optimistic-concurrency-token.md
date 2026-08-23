# Add an Optimistic Concurrency Token to an Entity

**Category:** Entity Framework Core
**Use when:** Concurrent edits to the same record are causing silent overwrites (last-write-wins data loss).

## Prompt

Concurrent updates to [entity name] are overwriting each other's changes silently. Add proper optimistic concurrency control. Use the analyze -> propose -> approve -> implement -> test -> review workflow and wait for my approval before editing code.

Analyze:
1. Locate the entity class and its current Fluent API/attribute configuration.
2. Confirm the target database (SQL Server `rowversion`, PostgreSQL `xmin`, etc.) so the right concurrency token mechanism is used.
3. Check every code path that loads and later saves this entity (including any "load, detach, reattach" patterns, bulk update paths, or API PATCH endpoints) since all of them need to carry the token through the round trip.

Propose:
- For SQL Server: add a `byte[] RowVersion` property mapped with `.IsRowVersion()` (Fluent API) or `[Timestamp]`, and confirm the migration adds a `rowversion`/`timestamp` column (not nullable, no default needed).
- For providers without native rowversion support, propose a manually-maintained `int ConcurrencyToken`/`DateTime LastModified` column configured with `.IsConcurrencyToken()`, and note it requires the app to increment/stamp it on every update (an interceptor or SaveChanges override is a good place to keep this consistent).
- Show exactly where `DbUpdateConcurrencyException` should be caught (repository layer or the calling service), and propose the resolution strategy: reload current values and ask the client to retry (409 Conflict at the API layer), or a merge strategy if applicable. Do not silently overwrite on conflict.
- Confirm the concurrency token must be sent back to the client (e.g., in the DTO/ETag) and returned by the client on update, otherwise the check is meaningless.

Wait for approval.

Implement the model change, migration, exception handling, and any DTO/API contract changes needed to round-trip the token.

Test: write a test that loads the same entity twice, updates and saves the first copy, then attempts to save the second stale copy and asserts `DbUpdateConcurrencyException` is thrown and handled correctly (e.g., surfaced as 409).

Review: confirm no code path bypasses the token (e.g., raw SQL updates or `ExecuteUpdate` calls that skip SaveChanges).
