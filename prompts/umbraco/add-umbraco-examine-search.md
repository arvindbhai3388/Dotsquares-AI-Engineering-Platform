# Add Examine Search Indexing and Results Page

**Category:** Umbraco CMS
**Use when:** A site needs on-site search across specific content types.

## Prompt

I need to add or extend Examine (Umbraco's Lucene-based indexing) to make specific Document Types searchable, and build a search results page/partial that queries that index. First check the existing Examine configuration: is there a custom index already defined (via `IIndexPopulator`/`IIndexDiagnostics` or configuration in `Program.cs`/composer classes) beyond Umbraco's default `ExternalIndex`, and are any Document Types already excluded from indexing via existing `IValueSetValidator`/index filters?

Propose the plan before implementing:
1. Whether to extend the default `ExternalIndex` (simplest, if this just needs the standard published-content index with maybe a couple of extra indexed fields) or create a dedicated custom index scoped to specific Document Type aliases (better if this needs different analyzers, different refresh timing, or excludes most site content).
2. Which properties need to be indexed as searchable text versus stored-only for display in results (e.g., index a plain-text-stripped version of a Richtext Editor field, not the raw HTML), using an `IValueSetValidator`/field definition to control this.
3. The search query approach: `IExamineManager`/`ISearcher` with a `.NativeQuery()` or fluent `Query()` builder, whether this needs fuzzy matching, field-boosting (e.g., title matches ranked above body matches), and pagination via `.SelectPage()`.
4. Mapping search results (which return `ISearchResult` with raw stored field values) back to typed view models or `IPublishedContent` via `IPublishedContentQuery.Content(id)` for rendering richer result cards -- note the N+1 risk if this re-fetches each result individually and batch appropriately.
5. **Index freshness**: Examine's default index updates on publish/unpublish via Umbraco's content cache refresh notifications, so confirm no custom index-rebuild-on-timer is needed; only add manual `RebuildIndex()` triggers if this is a genuinely custom index with populators that don't already hook into that pipeline.

Wait for approval, then implement the index configuration, the search Surface/API controller, and the results view with highlighting/excerpting if required. Validate: indexed content appears in results only after publish (not while only saved as draft), unpublishing removes it from results, empty-query and no-results states render sensibly, and pagination works correctly across multiple result pages.
