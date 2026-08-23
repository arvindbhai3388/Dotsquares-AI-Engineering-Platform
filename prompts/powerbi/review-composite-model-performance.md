# Review a Composite/DirectQuery Model for Performance Issues

**Category:** Power BI
**Use when:** An embedded report feels slow, especially on interaction/filter changes.

## Prompt

Review a composite or DirectQuery Power BI model backing an embedded report that users report as feeling slow, particularly when changing filters/slicers, and identify the specific performance issue(s) before proposing fixes. This is primarily a data-model/DAX diagnostic task rather than application code, but the application layer (embed configuration, query interaction patterns) can also contribute, so check both.

Diagnostic steps, in order:
1. **Confirm the model type and mixed-mode boundaries:** identify which tables are Import vs DirectQuery in the composite model, since every visual touching a DirectQuery table re-queries the underlying source on every filter/slicer interaction, while Import tables are served from the in-memory cache -- a single visual mixing both modes forces a live query even for the Import-mode portion.
2. **Check for query folding failures:** for DirectQuery tables, confirm whether transformations applied in Power Query still fold down to the native query (visible via "View Native Query" in Power BI Desktop) -- a broken folding chain (e.g. a step that can't be pushed to the source) forces Power BI to pull far more data than necessary and process it locally, which is a common silent cause of slow interactions.
3. **Use Performance Analyzer:** in Power BI Desktop, run Performance Analyzer while reproducing the slow interaction, and separate "DAX query" time from "Visual display" time from "Other" (e.g. cross-filtering) -- this tells you whether the bottleneck is the underlying source query, the DAX measure's complexity, or rendering, since each has a different fix.
4. **Check relationship cardinality and direction:** many-to-many relationships or bidirectional cross-filtering across large DirectQuery tables are a frequent cause of unexpectedly expensive generated queries; confirm relationships are as restrictive (single-direction, many-to-one) as the report's actual filtering needs allow.
5. **Check the source database side:** for DirectQuery against SQL Server or similar, confirm appropriate indexes exist for the columns Power BI is filtering/joining on -- Power BI can't fix a missing index on the source, and this is often the actual root cause when "everything else looks fine" in Power BI's own diagnostics.
6. **Application-layer check:** confirm the embed configuration isn't forcing unnecessary re-renders (e.g. re-embedding the entire report on every filter change from application code instead of using the SDK's `updateFilters`/visual-level filter APIs).

Summarize findings with the specific bottleneck identified and a proposed fix (model change, DAX rewrite, index addition, or embed-code change) before implementing anything, since model/DAX changes need review and re-testing of RLS and existing visuals.
