# Integrate External SQL Server Data via a Dataverse Virtual Table

**Category:** Power Apps / Power Platform
**Use when:** An app needs to show/edit data that must remain the system of record in an external SQL database.

## Prompt

Design and implement a Dataverse virtual table integration so the model-driven/canvas app I specify can display and edit data that lives in the external SQL Server database I point you to, without duplicating that data into native Dataverse tables. Start by confirming the exact SQL table/view involved, its primary key, which columns need to be read-only vs. editable from the app, and expected data volume/query patterns (virtual tables are not a good fit for very high-volume or highly transactional tables -- flag this if the numbers look wrong for the pattern).

Cover the required pieces:
- **Data provider choice**: recommend the OData v4 data provider (has an out-of-box connector for many REST-exposed SQL data) versus a custom data provider plugin implementing `IDataSource`/`IExecuteMultipleDataSource`-style CRUD methods for full control -- state which fits based on whether the SQL data is already exposed via an API/OData endpoint or needs a bespoke provider. If a custom provider is needed, confirm whether the existing .NET Web API in this solution could be extended to serve an OData v4 endpoint for that table instead of writing a from-scratch provider.
- **Virtual entity table definition**: the external-entity-name mapping, primary key mapping, and column mappings between Dataverse attribute schema names and the actual SQL column names -- get these exactly right, since a mismatch here fails silently as blank/erroring columns in the app rather than an obvious error.
- **Read vs. write support**: virtual tables are read-only by default; if edit support is required, confirm the data provider implements Create/Update/Delete and that this maps to real transactions against the SQL system of record without violating whatever constraints/business logic that system already enforces outside of Dataverse (a virtual-table edit bypassing SQL-side validation is a common integration bug).
- **Security and performance**: virtual tables don't support the full set of Dataverse security features (e.g. field-level security, some relationship types) -- call out any gap relevant to this specific table, and note that every grid/view render triggers a live call to the external source, so filtering/paging needs to be efficient at the SQL end.
- **ALM**: virtual entity data source and table definitions are solution components -- confirm they're added to the correct solution, and that the external connection's credentials are supplied via environment-specific configuration, not hardcoded per environment.

Propose the provider choice and mapping table, get my approval (especially on read-only vs. editable), then implement, and validate against a Dev copy of the SQL table before pointing at Test/Prod data.
