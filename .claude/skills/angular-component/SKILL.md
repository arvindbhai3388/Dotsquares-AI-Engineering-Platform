---
name: angular-component
description: >
  Use to scaffold a new Angular component correctly end to end — a
  standalone component with typed inputs/outputs, OnPush change detection
  where appropriate, and a TestBed test. Trigger phrases: "create a new
  Angular component", "scaffold a component for X", "add a reusable
  Angular component". For general fixes/changes to existing components,
  prefer the angular-developer agent; use this skill specifically when
  standing up a brand-new component from nothing.
---

# Angular Component Scaffolding Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

A new component should be usable, testable, and correctly typed from the
moment it's created — this skill walks through each of those concerns
explicitly rather than leaving them to be discovered later.

## Step 1 — Confirm dependencies and project conventions

- Confirm the Angular CLI is available in the target project
  (`ng version` or `package.json`'s `@angular/cli` devDependency) —
  scaffold by hand matching existing file conventions if the CLI isn't
  set up rather than blocking on it.
- Confirm whether the project is standalone-component-based
  (`bootstrapApplication` in `main.ts`) or NgModule-based
  (`AppModule`/`declarations`) — a new component in a standalone project
  should be `standalone: true` with its own `imports`; a new component in
  an NgModule project should be declared on the relevant module instead
  (see angular-developer for the full standalone-vs-NgModule guidance if
  that choice itself is in question).
- Confirm whether the project already leans on signals or RxJS-backed
  state for component-local reactivity, and match it — don't introduce
  the other pattern for this one component.

## Step 2 — Plan the component boundary

- Define the component's single responsibility before writing any code —
  if it's doing two unrelated things, split it into two components now
  rather than after it's grown further.
- Decide what's an **`@Input()`** (data/config passed in by the parent),
  what's **owned state** (internal to the component, signal or field),
  and what needs to **bubble up** via **`@Output()`** — get this boundary
  right before writing code; retrofitting it later touches every call
  site.

## Step 3 — Define the typed input/output contract

```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './example.component.html',
})
export class ExampleComponent {
  @Input({ required: true }) value!: string;
  @Output() valueChange = new EventEmitter<string>();
}
```

- Type every `@Input()`/`@Output()` explicitly — no `any`. Use
  `@Input({ required: true })` (Angular 16+) for inputs that aren't
  meaningfully optional, so a missing binding is a compile-time error
  instead of a runtime `undefined`.
- Default to `ChangeDetectionStrategy.OnPush` unless the project's
  existing components consistently use default change detection — OnPush
  matters most once the component sits in a list/table or receives
  frequently-changing inputs; see angular-developer for exactly what
  triggers a re-render under `OnPush`.
- Name an `@Output()` for two-way `[(x)]` binding support as `xChange`
  paired with an `@Input() x` — the naming convention is load-bearing,
  not stylistic, exactly as `ValueChanged`/`Value` is in Blazor.

## Step 4 — Implement the component logic

- One-time setup → `ngOnInit` (not the constructor, which should stay
  limited to DI).
- Prefer signals (`signal()`, `computed()`) for new component-local
  reactive state in a project that already uses them; otherwise match the
  project's existing RxJS-based pattern.
- If the component subscribes manually to any `Observable` that isn't
  handled by the `async` pipe, tie it to the component's lifetime with
  `takeUntilDestroyed()` or a `destroy$` `Subject` completed in
  `ngOnDestroy` — do this in the same step as adding the subscription,
  not as an afterthought; a manual subscription without teardown is an
  immediate defect.
- Put any API call behind an injected service, not directly in the
  component.

## Step 5 — Write the template

- Rely on Angular's default interpolation/binding escaping for any
  user-supplied content; never route untrusted content through
  `DomSanitizer.bypassSecurityTrustHtml` or similar.
- Prefer the `async` pipe over manual `.subscribe()` for anything only
  needed in the template.
- Keep non-trivial branching logic in the component class (a `computed()`
  signal or a method called from the template), not deeply nested
  `*ngIf`/`*ngFor` combinations in the markup itself.

## Step 6 — Write a TestBed test

```typescript
describe('ExampleComponent', () => {
  let fixture: ComponentFixture<ExampleComponent>;
  let component: ExampleComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ExampleComponent);
    component = fixture.componentInstance;
  });

  it('renders with the initial input', () => {
    fixture.componentRef.setInput('value', 'hello');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('hello');
  });

  it('re-renders when the input changes', () => {
    fixture.componentRef.setInput('value', 'first');
    fixture.detectChanges();
    fixture.componentRef.setInput('value', 'second');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('second');
  });

  it('emits valueChange when the user interacts', () => {
    let emitted: string | undefined;
    component.valueChange.subscribe((v: string) => (emitted = v));
    fixture.detectChanges();
    fixture.nativeElement.querySelector('button').click();
    expect(emitted).toBe('expected value');
  });
});
```

Cover, at minimum:

- Initial render with typical input values.
- An input change (`fixture.componentRef.setInput(...)` +
  `fixture.detectChanges()`) producing the expected re-render — check
  `package.json`'s Angular version; `setInput` needs Angular 14.1+, older
  projects set the property directly and call `detectChanges()`.
- A user interaction (a native `.click()` on the rendered element, not a
  direct method call) triggering the expected `@Output()` emission.
- If the component calls a service that hits `HttpClient`, mock it via
  `HttpClientTestingModule`/`HttpTestingController` rather than making a
  real request.

Confirm whether the project runs Jasmine + Karma (`ng test`, Angular's
traditional default) or has migrated to Jest before assuming syntax — the
`describe`/`it`/`expect` shape above is common to both, but runner
configuration and `spyOn`/`jest.fn()` mocking specifics differ.

## Step 7 — Review

- Confirm every `@Input()`/`@Output()` is explicitly typed, with no
  `any`.
- Confirm any manual subscription has matching teardown
  (`takeUntilDestroyed()` or `ngOnDestroy`).
- Confirm two-way binding naming is exact (`xChange` for `@Input() x`) if
  `[(x)]` support is intended.
- Confirm no `DomSanitizer.bypassSecurityTrust*` call was introduced for
  untrusted content.
- Run the TestBed test for real before calling the component done.

## Do
- Decide the input/owned-state/output boundary before writing the
  template.
- Type every input, output, and service call explicitly.
- Tie every manual subscription to the component's lifecycle.
- Write and run a TestBed test covering render, input change, and output
  emission from a user interaction.

## Don't
- Don't leave an `@Input()`/`@Output()` typed as `any`.
- Don't leave a manual subscription without teardown.
- Don't bind two-way `[(x)]` support with mismatched `x`/`xChange`
  naming.
- Don't call a `DomSanitizer.bypassSecurityTrust*` method on untrusted
  content.
- Don't claim the component works without running its test.
