# Decide Between InMemory, SQLite In-Memory, and a Real Test Database

**Category:** Entity Framework Core
**Use when:** Setting up a new test project, or fixing tests that pass against InMemory but fail against real SQL Server.

## Prompt

I need to set up (or fix) integration tests for [name the DbContext/project], and want the right test-database strategy rather than defaulting to whatever's fastest to wire up. [If applicable: "Tests are currently passing against the EF Core InMemory provider but failing against real SQL Server in [environment]."] Follow analyze -> propose -> approve -> implement -> test -> review; confirm the chosen strategy before writing test infrastructure.

Analyze:
1. Identify what the tests actually need to verify: pure LINQ-to-Objects-translatable query logic and basic CRUD (a weaker guarantee is acceptable), versus provider-specific behavior -- raw SQL, `ExecuteUpdate`/`ExecuteDelete`, database-generated defaults/computed columns, check constraints, cascade delete behavior, concurrency tokens, specific-provider translations (e.g., SQL Server `DATEDIFF`), or transaction/isolation semantics (none of which the InMemory provider enforces or even supports correctly).
2. If tests currently use the InMemory provider and are diverging from real-database behavior, pinpoint exactly which behavior differs (InMemory doesn't enforce required fields, unique constraints, or real relational cascade/FK behavior the way a real relational engine does, and it silently allows some invalid states).
3. Check whether SQLite (`Microsoft.EntityFrameworkCore.Sqlite` with a `":memory:"` or shared-cache connection) is viable given the provider-specific SQL/features actually used in the target DbContext (SQLite has different type affinity, limited `ALTER TABLE`, and doesn't support every SQL Server-specific feature (e.g., certain computed columns, `rowversion`), so provider-specific tests still won't be fully faithful).

Propose, matched to what's actually being tested:
- EF Core InMemory: acceptable only for testing pure query composition/business logic with no dependency on real relational constraint enforcement; explicitly call out that it should NOT be used to validate migrations, constraints, cascade behavior, or raw SQL.
- SQLite in-memory: a better default for most repository/service-layer integration tests needing real relational behavior (FKs, constraints, transactions) without a real server dependency, with the caveat above about SQL Server-specific feature gaps.
- A real (containerized, e.g. Testcontainers, or a dedicated test instance) SQL Server database: required for anything testing migrations themselves, provider-specific SQL, or exact production-parity behavior; propose this specifically for the tests that are currently failing due to a real behavioral difference.

Wait for approval on the strategy (which may mix approaches by test category) before implementing.

Implement the test fixture/base class for the chosen provider(s), following this repo's existing test project conventions.

Test: confirm the previously-InMemory-passing-but-SQL-Server-failing test now correctly fails or passes against the more faithful provider, proving it catches the real issue.

Review: confirm test setup/teardown properly isolates each test (fresh schema or transaction rollback per test) to avoid cross-test data leakage.
