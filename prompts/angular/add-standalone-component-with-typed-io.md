# Create a Standalone Component with Typed Inputs/Outputs

**Category:** Angular
**Use when:** Building a new reusable UI piece from a spec, or extracting duplicated template/logic into its own component.

## Prompt

Analyze the surrounding feature area first, then propose a plan for a new standalone Angular component before writing any code — follow the analyze -> propose -> approve -> implement -> test -> review workflow and wait for my go-ahead before implementing.

Build the component as `standalone` (no NgModule), importing only the `imports: []` it actually uses (`CommonModule` pieces, `ReactiveFormsModule`, other standalone components/directives/pipes) rather than a blanket `CommonModule` import out of habit. Prefer the signal-based `input()`/`input.required<T>()` and `output<T>()` functions from `@angular/core` over the `@Input()`/`@Output()` decorators for new components (use `model<T>()` instead when you need two-way binding), and give every input/output an explicit generic type — no `any`. Use the built-in control-flow syntax (`@if`, `@for` with a `track` expression, `@switch`) in the template instead of `*ngIf`/`*ngFor` structural directives.

Set `changeDetection: ChangeDetectionStrategy.OnPush` unless there's a specific reason not to, and use `inject()` at property-initializer scope for any dependencies instead of constructor parameter injection, matching whichever style is already dominant in this codebase — ask if the two are mixed and it's unclear which to follow. If the component needs derived state, compute it with `computed()` rather than a getter re-evaluated on every change-detection pass.

For a component that renders a projected template per item, expose an `<ng-template>`/`ContentChild` extension point or accept a `TemplateRef` input matching the pattern already used elsewhere in this codebase, rather than inventing a new templating convention.

Write a TestBed-based test alongside the component: configure `TestBed.configureTestingModule({ imports: [YourStandaloneComponent] })` (a standalone component is imported directly, not declared), create the fixture, set required inputs via `fixture.componentRef.setInput(...)`, call `fixture.detectChanges()`, and assert rendered output via `fixture.debugElement.query(By.css(...))`. Add a test that clicks/dispatches an event and asserts the corresponding output emitted the expected value (spy on `.subscribe()` or use `output.emit` assertions). Confirm the test suite runs and passes before reporting completion.
