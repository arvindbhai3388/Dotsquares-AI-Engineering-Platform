# Diagnose an Implicit Conversion Silently Causing an Index Scan

**Category:** SQL Server
**Use when:** A parameterized query with what looks like the right index is still producing a scan instead of a seek.

## Prompt

Diagnose whether an implicit data type conversion is the reason the attached query scans instead of seeking, despite an apparently matching index on the filtered/joined column. Pull the actual execution plan and look specifically for a `CONVERT_IMPLICIT` expression in the predicate or a yellow warning triangle on the Scan/Seek operator — this is the definitive signal, not a guess. If found, identify exactly which side of the comparison is being converted: compare the column's declared type in `sys.columns`/`INFORMATION_SCHEMA.COLUMNS` (including length, precision, and collation) against the type SQL Server assigns to the parameter or literal (e.g., an `nvarchar` parameter compared to a `varchar` column, an application passing a .NET `string` that ADO.NET defaults to `NVARCHAR` against a `VARCHAR` column, or an `int` parameter compared to a `decimal`/`numeric` column).

Explain precisely why this defeats the index: when the column itself must be converted to match the other side's type (per SQL Server's data type precedence rules), the optimizer can no longer seek using the column's native-type index, so it scans and converts every row to evaluate the predicate — this is easy to miss because the query "looks" correctly indexed and parameterized.

Fix at the source rather than working around it: correct the parameter/column type mismatch in the application code (explicitly set `SqlParameter.SqlDbType` to match the column type, e.g., `SqlDbType.VarChar` instead of letting ADO.NET default a string to `NVarChar`), or in the stored procedure's parameter declaration, or align a genuine schema type mismatch by changing the column type if that's the actual bug (treat a column type change as a schema change requiring the full backward-compatibility review, not a quick patch). Avoid the workaround of wrapping the column in an explicit `CONVERT()` in the WHERE clause — that just moves the non-sargable conversion onto the column side and still scans.

Show the before/after parameter declaration or column type, and the resulting plan change (scan to seek). Do not modify a production column's data type or run any fix against production yourself. Propose the change and get approval before applying anything to production, since a column type change may require a backward-compatible expand/contract migration.
