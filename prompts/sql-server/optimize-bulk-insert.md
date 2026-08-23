# Optimize a Slow Bulk Insert Path

**Category:** SQL Server
**Use when:** Importing or inserting large batches of rows via row-by-row inserts is too slow.

## Prompt

Diagnose the attached bulk-insert code path and replace row-by-row (RBAR) inserts with an appropriate set-based bulk mechanism. First confirm the actual pattern in use — a loop issuing individual `INSERT` statements (one round trip and one transaction per row, or per small batch), each incurring full statement compilation/execution overhead, log flushes, and index maintenance per row — and quantify the batch size involved, since the right fix differs for a few hundred rows vs. hundreds of thousands.

For .NET code inserting from an in-memory collection, propose `SqlBulkCopy` for large, simple bulk loads with minimal per-row transformation (set `BatchSize`, `BulkCopyTimeout`, and consider `SqlBulkCopyOptions.TableLock` for a dedicated load window vs. its concurrency cost on a live table), or a table-valued parameter (TVP) passed to a stored procedure when the insert needs business logic, validation, or an upsert (`MERGE`) against existing rows inside the same call — define the TVP type to match the target shape and reuse the project's existing data-access pattern (this project's existing ADO.NET parameter-helper (if one exists), or raw `SqlClient`/`Microsoft.Data.SqlClient` in the worker services) rather than introducing a new one.

Address the surrounding factors that matter as much as the insert mechanism itself: whether the target table has indexes/triggers that make every row insert more expensive than necessary during a bulk load (consider whether it's safe to disable non-clustered indexes and rebuild them after the load, only if this is an offline/maintenance-window load, not a live table); whether the load should be batched into chunks (e.g., 1,000-5,000 rows per batch) to avoid one giant transaction growing the log excessively and holding locks for the whole duration; and the table's recovery model/log growth impact for a very large one-time load.

Provide the before/after code, the expected relative throughput improvement and why, and the specific SqlBulkCopy/TVP configuration recommended. Do not run the bulk load against production data yourself. Propose the change, recommend validating against a representative-sized non-production dataset first, and get approval before executing anything against production.
