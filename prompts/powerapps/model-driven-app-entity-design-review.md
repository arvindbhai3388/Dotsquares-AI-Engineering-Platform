# Review a Dataverse Entity Design Before Building the Model-Driven App

**Category:** Power Apps / Power Platform
**Use when:** Designing new tables before building the model-driven app around them.

## Prompt

Review the Dataverse table/entity design I describe (or the solution's customizations.xml / table definitions if they already exist in this repo) before any model-driven app work begins on top of it. Treat this as a design review, not an implementation task -- do not create or modify tables yet.

Evaluate the design against:
- **Normalization**: flag repeated attribute groups that should be split into a related table, and flag over-normalization that will force excessive lookups/joins in views and Power Fx formulas for no real benefit.
- **Relationships**: for each 1:N, N:1, and N:N relationship, confirm the cascade behavior (Assign, Share, Delete, Merge, Reparent) is intentional -- Dataverse's default cascade-delete behavior is a common source of accidental mass data loss, so call out any relationship where "Remove Link" or "Restrict" would be safer than "Cascade All".
- **Choice/option-set fields**: recommend global choices over local option sets when the same values will be reused across tables, since local option sets can't be shared and complicate ALM.
- **Security roles**: map out which security roles will need Create/Read/Write/Delete/Append/Append To/Share/Assign privileges on each new table, at what level (User/BU/Parent-Child BU/Org), and flag any table that needs field-level security on specific columns (e.g. PII or financial data) -- this must be decided before the model-driven app's forms and views are built, since retrofitting field security after users depend on visible columns is disruptive.
- **Auditing and duplicate detection**: recommend whether auditing should be enabled per-table/per-field, and whether a duplicate detection rule is warranted given the natural key.
- **Naming and solution layering**: confirm the publisher prefix is consistent with the existing solution, and that new tables are added to the correct unmanaged solution for later export.

Produce your findings as a structured list (table name -> issue -> recommendation -> severity), not prose. For anything genuinely ambiguous (e.g. whether a relationship should cascade), ask me rather than guessing, since this is exactly the kind of decision this workflow expects a human to approve before implementation. Once I approve the design, hand off to the model-driven-app build itself as a separate follow-up task.
