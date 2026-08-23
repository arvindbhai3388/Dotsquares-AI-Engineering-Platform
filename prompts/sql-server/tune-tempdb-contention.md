# Diagnose and Tune TempDB Contention

**Category:** SQL Server
**Use when:** The server's wait statistics show heavy TempDB-related contention, e.g. PAGELATCH waits on allocation pages.

## Prompt

Diagnose the reported TempDB contention using wait statistics and propose a fix. Start from `sys.dm_os_wait_stats` (or `sys.dm_exec_session_wait_stats` for a targeted look) filtered to `PAGELATCH_UP`/`PAGELATCH_EX` waits, and cross-reference with `sys.dm_os_waiting_tasks` joined to `sys.dm_os_buffer_descriptors` or the resource description to confirm the contended pages are TempDB allocation pages (PFS, GAM, or SGAM pages, identifiable by page ID patterns like page 2:1:1 for PFS) rather than ordinary data-page latching, since the fix differs.

If it is TempDB allocation-page contention (the classic multi-file TempDB scenario under high concurrent temp-object creation/dropping), check the current TempDB configuration: number of data files (`sys.master_files` filtered to database_id 2), whether they're equally sized with matching autogrowth settings (unequal files cause the proportional-fill algorithm to favor one file, reintroducing the same contention on that file), and whether trace flag 1118 behavior (uniform extent allocation, now default behavior in modern SQL Server versions) is in effect. Recommend a data file count appropriate to the visible core count (commonly starting at 4, or matching logical cores up to 8, then reassessing rather than mechanically matching core count 1:1) and confirm they are pre-sized to a common size with equal, sane autogrowth increments to avoid renewed imbalance.

Also check whether the contention is actually driven by application/query behavior rather than a pure configuration gap: excessive use of temp tables/table variables in a hot loop, missing indexes causing large sort/hash spills into TempDB (correlate with the SpillToTempDb warning from the affected queries' execution plans), or long-running transactions preventing TempDB space reuse. Recommend fixing the root query/index issue in addition to, not instead of, the file configuration if both are contributing.

Present the diagnosis (with the specific wait stats and file configuration evidence), the recommended file count/sizing, and any query-level fixes. Do not change TempDB file configuration on production yourself, since it requires a service restart to take effect for file count changes — propose the change, the maintenance window it needs, and get explicit approval before applying it.
