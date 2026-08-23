# Convert Dynamic SQL to Parameterized SQL

**Category:** SQL Server
**Use when:** A stored procedure or application code builds a SQL statement via string concatenation.

## Prompt

Locate every place in the attached stored procedure (or application data-access code) where a SQL statement is built by concatenating strings, and convert each one to fully parameterized SQL. Treat this as both a security fix and a performance fix: string-concatenated SQL is vulnerable to SQL injection when any concatenated value can be influenced by user input, and it also defeats plan caching, since SQL Server treats each distinct literal-embedded statement text as a separate query needing its own compilation, bloating the plan cache and causing recompiles.

For T-SQL dynamic SQL (`EXEC(@sql)` or `EXEC sp_executesql`), convert to `sp_executesql` with a defined parameter list (`@ParamDefs`) and pass values as parameters rather than embedding them in the string — this applies even where dynamic SQL is legitimately needed for dynamic object names or optional predicates (e.g., building an optional WHERE clause based on which filters were supplied); keep the structural parts dynamic but the values parameterized. Where a table or column name must be dynamic, validate it against a known allow-list or `sys.objects`/`sys.columns` rather than trusting the caller, and use `QUOTENAME()` around any identifier that must be interpolated.

For application-side code (ADO.NET `SqlCommand`, or via this project's existing ADO.NET parameter-helper (if one exists), or raw `SqlClient`/`Microsoft.Data.SqlClient` in the worker services), replace string interpolation with `SqlParameter` objects added to `SqlCommand.Parameters`, reusing the existing helper pattern rather than introducing a new one. If the input is a set of rows (e.g., an IN-list built by concatenation), consider a table-valued parameter instead of parameterizing an arbitrary-length IN clause.

Show the before/after code, explain the injection vector that existed (with a concrete malicious-input example) and the plan-reuse benefit. Do not run any script against a live database as part of "testing" the fix — propose the change, note which test(s) should be added or updated to cover it, and wait for approval before executing anything beyond a local/dev environment.
