# Diagnose and Fix a Change-Detection Performance Issue

**Category:** Angular
**Use when:** A view feels sluggish, janky on typing/scrolling, or profiling shows excessive change-detection cycles.

## Prompt

Before changing anything, profile and report where the cost actually is — don't guess. Use the Angular DevTools profiler (or the Chrome Performance panel with Angular's `ng.profiler`/`ng.getComponent` if DevTools isn't available) to identify which component(s) run change detection most often and which template bindings take the longest per cycle, and report the specific culprits with evidence (component name, binding, cycle count/duration) rather than a general "this component seems heavy" impression.

Look specifically for these common causes and confirm which apply here: (1) a method call or object/array literal directly in a template expression (`[items]="filterItems()"`, `*ngFor="let x of getList()"`) that re-executes and re-allocates on every change-detection pass regardless of whether its inputs changed; (2) the component/its ancestors still on the default `ChangeDetectionStrategy` when the data flows in purely via `@Input()`/signals and `OnPush` would suffice; (3) a large `@for`/`*ngFor` list missing (or using a poor) `track`/`trackBy` expression, causing full DOM re-creation instead of in-place updates; (4) a `Subject`/interval firing far more often than the UI needs, driving change detection every time even when nothing visibly changes; (5) heavy synchronous work (formatting, sorting, filtering) done in a getter or template expression instead of memoized via `computed()` or precomputed once when the source data changes.

Propose the fix set before implementing, prioritized by measured impact: switch to `ChangeDetectionStrategy.OnPush` and ensure inputs are treated as immutable (new references on change, not in-place mutation) so `OnPush` actually detects updates; replace method calls in templates with `computed()` signals or a memoized pipe (a pure `Pipe` with `pure: true`, the default) that only recalculates when its arguments change by reference; add/fix `track` expressions on `@for` loops (an item's stable identity field, not array index); debounce/throttle high-frequency source streams with RxJS operators (`debounceTime`, `distinctUntilChanged`) before they reach the template.

After implementing, re-profile the same interaction and report the before/after cycle count or duration as evidence the fix worked, not just that the code compiles. Add a test only where the fix changed observable behavior (e.g. a `computed()` no longer recalculating for an unrelated input change) — a pure performance change without behavior change may not need a new test, but say so explicitly rather than silently skipping Validate.
