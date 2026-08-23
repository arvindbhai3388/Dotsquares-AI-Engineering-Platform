# Refactor a Class for Testability

**Category:** Code Review & Testing
**Use when:** Writing a test for existing code requires major workarounds because of tight coupling.

## Prompt

Analyze the class I specify and identify everything preventing it from being unit tested in isolation: static method calls to hard-to-fake APIs (`DateTime.Now`, `File.*`, `HttpClient` used directly, static utility classes with side effects), collaborators constructed with `new` inside the class instead of injected, singletons accessed via a global static instance, hidden I/O (direct file/network/database access buried in business logic), and any constructor or method that does real work (network calls, file writes) as a side effect of object construction.

Propose the smallest refactor that removes each obstacle, in this order of preference:

1. **Extract an interface and inject it** for anything that currently does I/O or wraps an external dependency, so tests can substitute a test double. Reuse an existing abstraction in the codebase if one already wraps the same concern -- do not create a second interface for something already abstracted elsewhere.
2. **Wrap non-injectable static calls** (`DateTime.Now`, `Guid.NewGuid()`) behind a small injectable time/ID provider interface, following whatever pattern (if any) the codebase already uses for this.
3. **Separate construction from side effects** -- if the constructor currently does work beyond assigning fields, move that work to an explicit method the caller invokes, or to a factory.
4. **Reduce hidden dependencies** on ambient/global state (`HttpContext.Current`, static caches) by passing what's needed explicitly instead.

For every proposed change, explain what specifically becomes testable that wasn't before, and confirm the refactor preserves existing external behavior -- this is a structural change, not a behavior change. Check for and preserve backward compatibility of any public API surface unless a breaking change is explicitly required and called out.

Present the plan before touching code: list each seam you intend to introduce and why. After I approve, implement the refactor, then write (or ask me to confirm you should write) unit tests that exercise the newly testable class using test doubles for the injected dependencies, proving the refactor achieved its goal. Do not perform unrelated renames, formatting changes, or additional refactors beyond what's needed to make the class testable.
