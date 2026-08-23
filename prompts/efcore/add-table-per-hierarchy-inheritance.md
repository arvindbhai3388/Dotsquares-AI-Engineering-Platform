# Configure Table-Per-Hierarchy (TPH) Inheritance Mapping

**Category:** Entity Framework Core
**Use when:** Several entity subtypes share a base type and should live in one table with a discriminator.

## Prompt

I have (or need to introduce) a class hierarchy -- [describe it, e.g. `Payment` base class with `CreditCardPayment`, `BankTransferPayment`, `WalletPayment` subclasses] -- and want to map it as Table-Per-Hierarchy (TPH), EF Core's default inheritance strategy, into a single table with a discriminator column. Use analyze -> propose -> approve -> implement -> test -> review; confirm the plan before touching code.

Analyze:
1. Confirm TPH is actually the right strategy here versus Table-Per-Type (TPT) or Table-Per-Concrete-Type (TPC): TPH is best when subtypes share most properties and queries commonly span the whole hierarchy; TPT/TPC are better when subtypes diverge heavily and per-type queries dominate (but TPT has known join-performance costs, and TPC duplicates shared columns per table).
2. Identify every property unique to each subtype -- in TPH these all become nullable columns on the single shared table, so check for any NOT NULL business constraints that now need to be enforced with check constraints or application-level validation instead of column-level constraints.
3. Check for existing enum-based "type" columns that might already be doing informal discrimination, to avoid a redundant/conflicting column.

Propose:
- Show the discriminator configuration: `modelBuilder.Entity<Payment>().HasDiscriminator<string>("PaymentType").HasValue<CreditCardPayment>("CreditCard").HasValue<BankTransferPayment>("BankTransfer")...`, and recommend explicit string values over relying on the default CLR type name (which breaks if a class is renamed).
- Flag the nullable-columns-per-subtype tradeoff explicitly and propose check constraints (via `HasCheckConstraint` in the entity configuration) for any subtype-specific required fields if data integrity at the DB level matters.
- Note the migration will alter/create one table with all subtype columns combined -- for an existing large table already containing polymorphic-ish data, plan the backfill of the discriminator column carefully (default value, then a data migration to set correct values per row).
- Confirm whether queries need `OfType<T>()`/pattern matching or if most queries should stay against the base type.

Wait for approval, then implement the model hierarchy, discriminator configuration, and migration (including check constraints and backfill logic).

Test: verify each subtype saves and loads correctly with the right discriminator value, and that querying the base type returns a correctly-typed mixed collection.

Review: confirm no subtype-specific required field can silently be left null due to the shared nullable-column structure.
