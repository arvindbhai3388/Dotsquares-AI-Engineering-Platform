# Add an Alternate Key to a Dataverse Table

**Category:** Power Apps / Power Platform
**Use when:** Integrating Dataverse with an external system of record.

## Prompt

Add a Dataverse alternate key to the table I specify, keyed on the external identifier column I provide (e.g. `ExternalSystemId`, or a composite of two columns), so external systems can upsert Dataverse records by that natural key instead of resolving the Dataverse GUID first. Before implementing, confirm: the table's logical name, which existing column(s) hold the external ID (or whether a new column needs to be added first), and whether uniqueness is actually guaranteed for that value across all expected records -- an alternate key that collides breaks every future upsert with a hard error, so this needs real verification, not assumption.

Cover both sides of the integration:
- **Dataverse-side definition**: the alternate key's schema name, the underlying column(s) and their type/length/nullability, and confirm Dataverse will build the supporting unique index (this can take time on tables with existing data and will fail outright if duplicate values already exist -- recommend running a duplicate-check query first).
- **Consuming code**: show how to construct an `EntityReference` using the alternate key syntax (`new EntityReference("entityname", "keyname", value)` for the SDK, or the `entityset(keyname='value')` URL syntax for the Web API) so the external system's integration code (or the .NET service calling into Dataverse) can `Upsert`/`Update`/`Retrieve` without a prior lookup round-trip.
- **Performance note**: alternate-key lookups use the supporting index and are efficient, but confirm this isn't being used as a substitute for a real primary key relationship where a lookup/relationship field would be more appropriate.
- **ALM**: alternate keys are solution-aware components -- confirm the key gets added to the correct solution so it deploys with the rest of the customizations to Test/Prod, and flag that if Prod already has conflicting duplicate data, the key creation will fail on import until cleaned up.

Propose the key definition and the consuming-code changes together, wait for approval (especially on the uniqueness assumption), then implement, and write/update a test that exercises the upsert-by-alternate-key path against a faked/mocked `IOrganizationService` or HTTP handler.
