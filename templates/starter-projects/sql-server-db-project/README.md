# Starter Scaffold — SQL Server Database Project (SSDT)

> Template outline for bootstrapping a SQL Server Database Project (SSDT `.sqlproj`) for
> schema-as-code, as an alternative or complement to EF Core migrations — common where a
> client wants the schema independently versioned/reviewed from application code, or where
> the app layer is legacy (EF6 Database-First, raw ADO.NET). This is a folder-structure and
> setup guide, not a working demo.

## Recommended Folder Structure

```text
<ProjectName>.Database/
├── <ProjectName>.Database.sqlproj
├── Tables/
│   └── <Schema>.<Table>.sql          # One CREATE TABLE per file, named <Schema>.<Table>.sql
├── Views/
│   └── <Schema>.<View>.sql
├── StoredProcedures/
│   └── <Schema>.<Procedure>.sql
├── Functions/
│   └── <Schema>.<Function>.sql
├── Security/
│   ├── <Schema>.sql                  # CREATE SCHEMA statements
│   └── Roles/
├── Scripts/
│   ├── Pre-Deployment/
│   │   └── Script.PreDeployment.sql
│   └── Post-Deployment/
│       └── Script.PostDeployment.sql # Idempotent seed/reference data only — no PII, no secrets
└── <ProjectName>.publish.xml         # Publish profile — target connection string as placeholder only
```

## Key Tooling

| Tool/Package | Purpose |
|---|---|
| SQL Server Data Tools (SSDT) / `Microsoft.Build.Sql` SDK-style project | Schema-as-code project format |
| `sqlpackage` (CLI) | Build/publish `.dacpac` outside Visual Studio, e.g. in CI |
| `tSQLt` (only if the client wants SQL-level unit tests) | T-SQL unit testing framework |

## First Things to Configure

1. Decide SSDT vs. EF Core migrations vs. both as the source of truth for schema —
   maintaining two independent sources of truth for the same schema is a common source of
   drift; document the decision in the client's `CLAUDE.md` §4.7.
2. One object per file, named to match the object (`Schema.Table.sql`), matching SSDT
   convention — avoids merge conflicts and keeps diffs reviewable.
3. Keep the publish profile's connection string as a placeholder (`<TARGET_CONNECTION_STRING>`)
   — never commit a real one; supply it via a pipeline variable/secret at publish time.
4. Use pre/post-deployment scripts only for idempotent operations (`MERGE`, `IF NOT EXISTS`
   guards) — never a script that fails or duplicates data on a second run.
5. All stored procedures/functions take parameters — never build dynamic SQL from
   unparameterized string concatenation, even inside the database project itself.
6. If tSQLt or an equivalent is adopted, set it up before writing non-trivial stored
   procedure logic (Test-First applies to SQL objects too, when the client's process calls
   for it).
