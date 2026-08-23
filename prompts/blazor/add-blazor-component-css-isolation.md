# Add CSS Isolation to a Component

**Category:** Blazor
**Use when:** A component's styles leak into other components, or are being overridden unpredictably by global stylesheets.

## Prompt

Before implementing, read the component's current markup and identify which of its existing styles live in a global stylesheet versus inline `<style>` blocks versus already-isolated `.razor.css` files elsewhere in the codebase, and confirm the scope of the migration (just this component, or this component plus its direct children) with me before making changes, since CSS isolation only scopes the component's own root elements by default, not descendant components' internals, unless `::deep` is used deliberately.

Create a `{ComponentName}.razor.css` file alongside the component and move the component-specific rules into it. Blazor rewrites each rule's selectors with a generated scope attribute (`b-xxxxxxxxxx`) at build time, so verify the build actually processes it (check the generated `{Assembly}.styles.css` bundle is referenced in the host page's `<head>`, typically via `<link rel="stylesheet" href="{Assembly}.styles.css" />`) — a common failure mode is adding the `.razor.css` file but forgetting the bundle isn't linked in a non-standard host page.

Use `::deep` explicitly and sparingly for the cases isolation intentionally doesn't cover: styling a child component's root element from the parent (e.g. `::deep .child-class { ... }`), or styling markup rendered via a `RenderFragment`/`ChildContent` passed in from outside, since content injected via a fragment is compiled as part of the *caller's* scope, not this component's. Do not use `::deep` as a blanket escape hatch to keep old broad selectors working unchanged — that defeats the purpose of isolating styles and often signals the rule should move to whichever component actually owns that markup.

If existing global styles targeted this component by a shared class name also used elsewhere, audit whether removing the global rule affects those other usages before deleting it — search for other consumers of that class name first. Keep CSS custom properties (variables) for cross-component theming (colors, spacing tokens) in a global stylesheet or a `:root`-scoped file, not inside an isolated `.razor.css`, since isolated files intentionally don't leak variables either. Validate visually (screenshot or manual check) that this component renders identically before and after isolation, and confirm no other component's styling regressed from removing the now-migrated global rules.
