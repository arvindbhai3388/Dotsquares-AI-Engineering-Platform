# Review a Component for Accessibility Issues

**Category:** React
**Use when:** Before shipping a new interactive component, or when accessibility feedback (screen reader, keyboard-only, or an automated audit) has flagged a problem.

## Prompt

Review the component (read-only pass first) against these categories and report findings with file/line references before making any changes — propose the fixes and get my sign-off before implementing, since some accessibility fixes change markup structure or visual behavior that's worth confirming first:

**Semantic HTML** — flag any interactive element built from a non-semantic tag (a `<div>`/`<span>` with an `onClick` acting as a button; a fake link without `href`) instead of the native element (`<button>`, `<a href>`) that gets keyboard focus, `Enter`/`Space` activation, and correct default role for free. Flag heading levels that skip (`<h2>` directly to `<h4>`) or aren't used for structure at all. Flag list content not marked up as `<ul>`/`<ol>`/`<li>`.

**ARIA usage** — flag ARIA added where a native semantic element would already convey the same information ("no ARIA is better than bad ARIA"). Where a custom widget genuinely needs ARIA (a custom dropdown, tabs, modal, combobox), check it matches the correct WAI-ARIA APG pattern for that widget (correct `role`, required `aria-*` attributes, and the expected keyboard interaction model for that pattern) rather than an ad hoc guess. Flag `aria-label`/`aria-labelledby` used to override visible text in a way that would mismatch what a sighted user reads vs. what a screen reader announces. Check form inputs have an associated accessible name via a `<label htmlFor>`/wrapping `<label>`, not a placeholder alone, and that validation errors are wired via `aria-describedby` and `aria-invalid` (see this category's form-validation prompt).

**Keyboard navigation** — verify every interactive element is reachable via `Tab` in a sensible order (flag any positive `tabIndex` value, which breaks natural order — `0` and `-1` are fine), that focus is visible (not suppressed via `outline: none` without a replacement focus style), and that custom widgets (modal, menu, dropdown) trap/restore focus correctly: focus moves into the widget on open, `Escape` closes it, and focus returns to the triggering element on close. Check that a modal/dialog uses `role="dialog"`/`aria-modal="true"` and that background content is inert to screen readers while open (via a library primitive already in use, e.g. Radix/Headless UI, or `inert`/`aria-hidden` toggled correctly if hand-rolled).

**Images and non-text content** — flag decorative images without `alt=""` (not a missing `alt`, which forces screen readers to announce the filename) and meaningful images without descriptive `alt` text. Flag icon-only buttons without an accessible name (`aria-label` or visually-hidden text).

**Automated + manual verification** — run this project's accessibility linting (`eslint-plugin-jsx-a11y` if configured) and note any suppressed rules worth revisiting. Where feasible, add an automated check via `jest-axe`/`vitest-axe`'s `toHaveNoViolations()` on the component's rendered output as a regression guard, but state clearly that this catches only a subset of issues (contrast, missing labels, invalid ARIA) and does not replace manual keyboard-only and screen-reader testing for interaction flow — call out which parts still need a human pass and by whom.
