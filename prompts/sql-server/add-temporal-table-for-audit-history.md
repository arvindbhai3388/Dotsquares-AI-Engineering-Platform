# Convert a Table to a System-Versioned Temporal Table

**Category:** SQL Server
**Use when:** A table needs point-in-time history or auditing and currently has no history mechanism (or relies on a hand-rolled audit table/trigger).

## Prompt

Propose converting the attached table to a SQL Server system-versioned temporal table so history is tracked automatically instead of via a manual audit table or trigger. Start by confirming the table's current state: primary key present, no existing `PERIOD FOR SYSTEM_TIME` columns, and whether any existing trigger-based or shadow-table audit mechanism needs to be retired as part of this change (call this out explicitly rather than leaving both running in parallel).

Design the change as `ALTER TABLE ... ADD SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START, SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END, PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime)` followed by `ALTER TABLE ... SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = schema.TableHistory))`, and specify an explicit history table name/schema rather than letting SQL Server auto-name it, so it's discoverable and can have its own retention/index strategy. Recommend a retention policy (`HISTORY_RETENTION_PERIOD`) appropriate to the table's audit/compliance requirement, and note the storage growth implication for a high-write table — every UPDATE/DELETE writes a full row copy to the history table, so a table with wide rows and frequent updates will grow the history table quickly; propose an index on the history table's period columns if point-in-time queries will be common.

Call out the concrete edge cases: adding the period columns to an existing large table is a schema change that can require a size-of-data operation (test the migration time on a representative copy first); existing application code doing `SELECT *` will now see the new period columns unless explicitly excluded, which can break serialization or column-order assumptions — audit callers before enabling; and temporal tables don't allow certain DDL (e.g., you can't `TRUNCATE` while versioning is on, and schema changes require temporarily suspending `SYSTEM_VERSIONING`).

Provide the full DDL, the retention/index recommendations, and the list of application call sites that need review for `SELECT *` impact. Do not run any of this DDL against a production database. Propose it, recommend testing the migration timing on a non-production copy first, and wait for explicit approval before applying anything to production.
