# Add Full-Text Search to Replace a Slow LIKE Search

**Category:** SQL Server
**Use when:** A `LIKE '%term%'` search is slow on a large table and doesn't support relevance ranking or multi-word matching.

## Prompt

Evaluate whether SQL Server Full-Text Search (FTS) is the right fix for the attached slow `LIKE '%term%'`-based search, and if so implement it. First confirm the diagnosis: a leading-wildcard `LIKE` predicate cannot use a standard B-tree index (it forces a scan regardless of any index on the column), so if the table is large and this search is frequent, FTS (or, for very simple prefix-only matching, a rewritten `LIKE 'term%'` with a supporting index) is the appropriate fix rather than trying to force a regular index to help a leading-wildcard search.

Check whether Full-Text Search is already installed/enabled on the instance (`SELECT SERVERPROPERTY('IsFullTextInstalled')`) and whether a full-text catalog already exists for this database before creating a new one. Design the full-text index on the target column(s) (`CREATE FULLTEXT INDEX ON table(column) KEY INDEX <unique_index_name> ON catalog_name`), noting that it requires a unique, non-nullable single-column index to key off, and choose the appropriate language/word-breaker (`LANGUAGE` argument) for the content being searched. Recommend a change-tracking mode (`AUTO`, `MANUAL`, or `OFF` with scheduled `ALTER FULLTEXT INDEX ... START UPDATE POPULATION`) based on how fresh search results need to be vs. the overhead of continuous population on a write-heavy table.

Rewrite the search query to use `CONTAINS()` for precise boolean/phrase/prefix term matching or `FREETEXT()` for looser natural-language relevance matching, choosing based on the actual search UX (exact phrase/boolean operators vs. "search-engine-like" fuzzy matching), and show how to surface relevance ranking via `CONTAINSTABLE`/`FREETEXTTABLE` joined back to the base table if the UI needs results ordered by relevance rather than an arbitrary column.

Call out the initial population cost for a large existing table (a full population can take significant time and I/O) and the ongoing maintenance overhead of the catalog. Provide the full DDL and the rewritten query. Do not create the full-text catalog/index or run the initial population against production yourself — propose it and get approval before running anything beyond a dev/test environment.
