# Resolve Conflicting EF Core Migrations After a Rebase/Merge

**Category:** Entity Framework Core
**Use when:** Two branches both added migrations against the same model snapshot, causing duplicate/out-of-order migration files.

## Prompt

After rebasing/merging [branch names if known], I have conflicting or duplicated EF Core migrations -- both branches generated a migration against the same prior model snapshot, so I likely have two migrations claiming to be "next," a conflicted `ModelSnapshot.cs`, or duplicate column/table changes. Help me resolve this safely. Follow analyze -> propose -> approve -> implement -> test -> review; do not delete or edit migration history until I approve the plan.

Analyze:
1. List all migration files in the `Migrations` folder with their timestamps/order, and identify which ones are genuinely new from each branch versus already applied to any shared/production database (check the `__EFMigrationsHistory` table if you have DB access, or ask me which migrations have already been deployed).
2. Diff the conflicted `<Context>ModelSnapshot.cs` to understand what each branch's migration actually changed at the model level, since the snapshot conflict is usually just a merge artifact of two migrations both editing the "current state" file.
3. Determine whether the two migrations touch the same table/column (a true logical conflict) or entirely different parts of the model (a mechanical ordering conflict only).

Propose:
- If migrations are NOT yet applied anywhere outside local dev: propose removing one branch's migration with `dotnet ef migrations remove` and regenerating it after the merge, so there's a single, correctly-ordered migration chain, then manually reconcile the snapshot.
- If a migration IS already applied to a shared/staging/production database: do NOT remove or renumber it -- propose keeping both migrations in sequence (reordering only the file timestamps/class ordering if needed so both apply cleanly) and manually resolving the snapshot file to reflect the union of both changes.
- If both migrations touch the same column/table incompatibly (true conflict), propose the specific manual edit needed to one migration's Up()/Down() to make them compose correctly, and flag it for careful review since this is the highest-risk case.
- Always propose ending with `dotnet ef migrations list` and a fresh `dotnet ef database update` against a disposable local/test database before declaring it resolved.

Wait for approval before touching any migration file.

Implement the approved resolution.

Test: apply the reconciled migrations to a fresh database from scratch, and if feasible, to a copy of a database that already has the pre-conflict migrations applied, confirming both paths reach the same final schema.

Review: confirm the final `ModelSnapshot.cs` accurately reflects the true combined model with nothing lost from either branch.
