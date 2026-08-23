# Add Efficient Query Support for a JSON Column

**Category:** SQL Server
**Use when:** Semi-structured JSON payloads are stored in an NVARCHAR column and need to be queried or filtered efficiently.

## Prompt

Design efficient query support for the attached table's JSON payload column (stored as `NVARCHAR(MAX)` or similar) so it can be filtered and indexed instead of only ever being deserialized application-side. Start by confirming the JSON is reasonably well-formed and stable in shape (validate with `ISJSON(column) = 1` as a check constraint if not already enforced, since malformed JSON silently breaks `JSON_VALUE`/`OPENJSON` calls at query time rather than failing fast at write time).

For simple scalar property lookups (e.g., filtering where a top-level or nested string/number property equals a value), use `JSON_VALUE(column, '$.path.to.property')` in the query, but recognize this alone still scans the whole table since it's a function call, not an indexed access — for any property that needs to be searched frequently or joined on, add a persisted computed column (`ALTER TABLE ... ADD PropCol AS JSON_VALUE(column, '$.path') PERSISTED`) and index that computed column, following the same pattern as indexing any other derived expression; rewrite queries to filter on the computed column rather than calling `JSON_VALUE` inline so the optimizer can match the index.

For querying array elements or multiple properties at once (e.g., "find rows where any item in an array matches"), use `OPENJSON(column, '$.arrayPath') WITH (...)` in a `CROSS APPLY`, and be explicit that `OPENJSON` is a table-valued function evaluated per row — for a large table this still requires either a supporting computed-column index on the properties actually filtered, or accepting that this is an occasional/reporting-style query rather than a hot path; do not present OPENJSON alone as a performance fix for a frequent, latency-sensitive query.

If the JSON structure is stable and richer, structured querying is needed regularly, evaluate whether specific properties should instead be promoted to real relational columns (with the JSON column kept for the remaining flexible/rarely-queried attributes) rather than indexing around JSON indefinitely — flag this as a larger design conversation, not something to silently decide.

Provide the check constraint, the specific computed columns and indexes proposed, and the rewritten queries. Do not apply any of the DDL against production. Propose the changes, note the backfill/validation needed for the `ISJSON` constraint on existing rows, and get approval before running anything against a production or production-like database.
