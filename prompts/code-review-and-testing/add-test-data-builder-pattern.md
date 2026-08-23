# Introduce a Test Data Builder Pattern

**Category:** Code Review & Testing
**Use when:** Test setup code is duplicated and breaks every time an entity gains a new required property.

## Prompt

Introduce a test data builder (or object mother, whichever fits the existing test project's conventions better -- check first) for the entity/DTO I specify, to replace the repetitive, brittle object construction currently scattered across its test files.

First, survey the existing tests that construct this type and confirm the actual pain: count how many places build it inline, and note how many needed editing the last time a property was added (if visible from history or from obviously stale-looking construction blocks). This justifies the refactor rather than adding abstraction speculatively.

Design the builder with:

- A fluent API (`new UserBuilder().WithEmail("x@y.com").WithRole(Role.Admin).Build()`) where every `With*` method returns the builder itself.
- Sensible, valid defaults for every property so `new UserBuilder().Build()` alone produces a valid, usable object for tests that don't care about that entity's specific field values -- this is the main point: adding a new required property to the entity means updating the builder's default once, not every call site.
- A `Build()` method returning the real production type -- the builder must not introduce a parallel/duplicate model.
- Optional convenience presets for common variants used repeatedly across tests (e.g., `UserBuilder.InactiveUser()`, `OrderBuilder.WithLineItems(3)`), only if genuinely reused three or more times -- don't pre-build presets speculatively for hypothetical future tests.

Place the builder in the existing test project's shared/test-helpers location if one exists; otherwise propose a location consistent with the project's structure and confirm before creating new folders.

After building it, migrate the existing tests that construct this entity inline to use the builder, verifying each migrated test still passes with the same effective behavior (same property values it depended on, explicitly set via `With*` rather than relying on the builder's default matching the old inline value by coincidence). Do not change what each test is actually asserting -- this is a construction-mechanics refactor, not a test-behavior change. Run the full affected test file(s) afterward and report actual results.
