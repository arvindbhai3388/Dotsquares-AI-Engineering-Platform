# Migrate an NgModule-Based Component to Standalone

**Category:** Angular
**Use when:** A component still declared in an `NgModule` needs to become standalone, either in isolation or as part of a broader NgModule-removal effort.

## Prompt

Before changing anything, analyze the component's current `NgModule` and report: every other declarable (component/directive/pipe) sharing that module, everything the module imports/exports that this component actually depends on versus what's incidental, anything that lazy-loads this module via `loadChildren`, and any other module that imports this one specifically to reuse this component. Propose a migration plan and wait for approval before editing code — a component with several siblings still coupled to the same module may need those siblings migrated together, or the module kept around as a thin re-export shim in the interim, and I want to choose which before you start.

Once approved, convert the component: add `standalone: true` (or omit the flag if this codebase's Angular version defaults to standalone and the flag is redundant — check `@angular/core` version first), replace the module-level `declarations`/`imports` this component relied on with an explicit `imports: []` array on the component itself, listing only what its own template uses. If the component used `CUSTOM_ELEMENTS_SCHEMA` or other schema overrides at the module level, decide whether it's actually needed on the component and don't carry it over by default.

Update every place that referenced this component through its old module — a route's `loadChildren` pointing at the module should become `loadComponent` pointing directly at the standalone component (for a single lazily-loaded component) or be adjusted to route into the remaining module if siblings aren't yet migrated; another standalone component/module that imported the old NgModule to get this component should import the component directly instead.

If the module becomes empty or vestigial after migration, propose removing it as a separate, explicitly called-out step rather than silently deleting it — deleting a module can break barrel exports or other consumers that aren't obvious from this component's own usage. Run the existing tests for this component and any consumer that changed its import, and confirm nothing regressed before reporting completion.
