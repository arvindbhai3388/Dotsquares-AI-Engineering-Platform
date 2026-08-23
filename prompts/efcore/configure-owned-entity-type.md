# Configure an Owned Entity Type for a Value Object

**Category:** Entity Framework Core
**Use when:** Modeling a value object like Address or Money that shouldn't have its own identity.

## Prompt

I need to model [describe the value object, e.g. `Address` on `Customer`, or `Money` (Amount + Currency) on `Invoice`] as a proper value object using EF Core's owned entity type support, rather than as a separate entity with its own primary key/identity. Follow analyze -> propose -> approve -> implement -> test -> review, and confirm the approach with me first.

Analyze:
1. Confirm this is genuinely a value object (no independent identity, always accessed through its owner, equality by value not by key) and not something that should instead be a first-class entity with its own table and FK relationship.
2. Check whether the owning entity needs exactly one instance (`OwnsOne`) or a collection of them (`OwnsMany`, e.g. multiple `Address` records per `Customer`).
3. Check whether the value object should be stored inline in the owner's table (default for `OwnsOne`) or in a separate table -- `OwnsMany` always requires a separate table since it's a collection.

Propose:
- Show the exact `modelBuilder.Entity<Owner>().OwnsOne(o => o.Address, a => { a.Property(x => x.Street)...; })` configuration, including explicit column naming if the value object's property names would otherwise collide with the owner's or need renaming for the underlying schema.
- For `OwnsMany`, show the shadow FK/key setup EF generates automatically and confirm whether an explicit key is needed.
- Note that owned types are always loaded with their owner (no separate `Include()` needed) but can't be queried independently via a `DbSet<T>` unless also exposed, and cannot easily be shared/referenced by multiple owners (that would call for a real entity instead).
- Flag null-handling: for `OwnsOne`, a null reference for the owned type either needs all its columns nullable or a non-null default depending on business rules -- clarify which is intended.
- Note the migration impact: inline owned types add columns to the owner's table; `OwnsMany` creates a new table with a foreign key back to the owner.

Wait for approval, then implement the configuration and value object class (as an immutable/equatable type if not already), plus the migration.

Test: verify save/load round-trips correctly, including the null/empty case, and that value equality (if implemented) behaves as expected.

Review: confirm no other code was still treating the value object as an independently queryable entity.
