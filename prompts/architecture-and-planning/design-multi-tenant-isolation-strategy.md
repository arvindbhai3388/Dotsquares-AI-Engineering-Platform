# Design a Multi-Tenant Data Isolation Strategy

**Category:** Architecture & Planning
**Use when:** Architecting a new multi-tenant feature or product from scratch.

## Prompt

Design a multi-tenant data isolation strategy for the scenario described below. Produce a design proposal for me to review and approve — do not create any database objects, migrations, or code as part of this task.

Cover the three standard isolation models explicitly, evaluated against the scenario's actual requirements (expected tenant count, expected data volume per tenant, whether tenants require independent scaling/backup/restore, cross-tenant reporting needs, and any compliance requirement for physical data separation between specific tenants):

1. **Shared database, row-level filtering** — a single schema with a tenant ID column and filtering (e.g., via a global query filter, a required predicate on every query, or row-level security) on every access path. Describe the isolation guarantee this actually provides, the concrete risk of a missing-filter bug leaking data across tenants, and how that risk would be mitigated (e.g., enforced at the data-access layer, not per-call-site).
2. **Schema-per-tenant** — one schema per tenant in a shared database instance. Describe migration/versioning complexity as tenant count grows, and connection/schema-switching overhead.
3. **Database-per-tenant** — full physical isolation. Describe the operational cost (backup, migration rollout, monitoring multiplied per tenant), and when this is actually justified (e.g., regulatory requirement, a small number of large enterprise tenants) versus overkill.

Then produce:

4. **Recommendation** — the model that fits this scenario's specific scale and requirements, with explicit reasoning tied to the factors above (not a generic "shared is cheaper" answer).
5. **Tenant identification and enforcement** — how the tenant context is established per request (claim/token, subdomain, header) and where isolation is enforced in the code (middleware, base repository, query filter) so it cannot be bypassed by a new feature forgetting to apply it.
6. **Cost and security trade-offs** — infrastructure cost comparison at the scenario's expected scale, and the blast radius of a bug or breach under each model.
7. **Migration path if outgrown** — how a tenant could later be moved to a more isolated model (e.g., a large tenant graduating from shared to dedicated) without a rewrite.
8. **Effort estimate** for implementing the recommended approach.

Wait for approval before implementing.
