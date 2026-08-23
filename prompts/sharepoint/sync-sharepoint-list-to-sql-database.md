# Sync a SharePoint List to a SQL Server Table

**Category:** SharePoint (Microsoft Graph)
**Use when:** A SharePoint list needs to be queryable/reportable from the main application database.

## Prompt

Build a scheduled, one-way sync job that pulls a SharePoint list into a SQL Server table so it can be queried/reported alongside this app's own data, handling adds, updates, and deletes idempotently on every run.

Requirements:
- Use delta query (see the delta-query-change-tracking prompt) as the underlying change-detection mechanism rather than pulling the entire list every run once the target list can grow large; for a first implementation on a small, rarely-changing list, a full pull with upsert-by-key is acceptable but say explicitly which approach was chosen and why.
- Design the target SQL table with the SharePoint item ID (and site/list ID if this job will ever sync more than one list into the same table) as a natural or composite key, plus a `LastModifiedDateTime`/`ETag` column used to detect and skip no-op updates.
- Implement upserts using this app's existing data-access approach — EF6/DbContext or raw ADO.NET with parameterized `MERGE` calls via this project's existing ADO.NET helper (if one exists), whichever this project already uses — do not introduce a new ORM or raw string-concatenated SQL.
- Handle deletions explicitly: when delta query reports an item as deleted, mark the corresponding SQL row as soft-deleted (or hard-delete, matching this app's existing convention for other synced/external data) rather than leaving stale rows with no way to distinguish "not yet synced" from "removed upstream."
- Make the whole sync run idempotent and safely re-runnable: a crash mid-run followed by a retry from the last successful checkpoint must not create duplicate rows or double-count updates — wrap each batch's SQL writes in a transaction scoped appropriately for the data-access technology in use.
- Schedule the job using this app's existing scheduling/worker mechanism (matching how your existing background-job/worker pattern or existing scheduled tasks are structured) rather than adding a new scheduling library.
- Log sync run summaries (items added/updated/deleted, duration, errors) without logging full field values that might be sensitive.
- Confirm least-privilege Graph read permissions are sufficient since this is a read-from-SharePoint, write-to-SQL flow only.

Follow the analyze -> propose -> approve -> implement -> test -> review workflow: propose the SQL table schema, key strategy, and delta-vs-full-pull decision first, then implement with tests covering initial full sync, incremental add/update/delete, and a simulated crash-and-resume scenario.
