# Configure a Value Converter for a Non-Native Column Type

**Category:** Entity Framework Core
**Use when:** A CLR type doesn't map naturally to a column type (enum-to-string, encrypted field, JSON blob, custom struct).

## Prompt

The property [name the property and entity, e.g. `Order.Status` (enum) or `Customer.TaxId` (needs encryption at rest)] needs a value converter because its CLR representation shouldn't be stored as-is in the database. Follow analyze -> propose -> approve -> implement -> test -> review.

Analyze:
1. Identify the current mapping (implicit conversion EF is already doing) and why it's insufficient -- e.g., storing an enum as an int is fragile across reordering, or a field needs encryption/masking before it hits the column.
2. Check whether this needs a simple `ValueConverter<TModel, TProvider>` or a full `ValueConverter` + `ValueComparer` (required when the property is a mutable reference type or collection, so EF's change tracker can correctly detect modifications).
3. Confirm whether the conversion needs to be queryable (used in a `Where()` clause) -- some converters (e.g., involving encryption) cannot be translated to SQL and will force client-side evaluation or throw at query time.

Propose:
- Show the exact `HasConversion(...)` Fluent API call, e.g. converting an enum to its string name for readability (`v => v.ToString(), v => (StatusEnum)Enum.Parse(typeof(StatusEnum), v)`), or a converter using `System.Text.Json` for a value object stored as JSON.
- If encryption is involved, do NOT propose storing keys in code or config covered by restricted-file rules; reference the existing secrets/key-management pattern used elsewhere in the codebase.
- Flag the required column type/size change (e.g., string enum needs a `nvarchar` column with sufficient length) and that this requires a migration.
- Note the ValueComparer requirement explicitly if the property type is mutable (e.g., a `List<T>` or custom class), since without it EF may not detect in-place mutations on SaveChanges.
- Warn that any existing data in the old format needs a data migration/backfill step, not just a mapping change.

Wait for approval, then implement the converter, the entity configuration, and the migration.

Test: unit test the converter round-trip (model value -> provider value -> model value) in isolation, plus an integration test that saves and reloads an entity through the real DbContext.

Review: confirm queries filtering on this property still work as expected (or are intentionally excluded from server-side translation with a documented reason).
