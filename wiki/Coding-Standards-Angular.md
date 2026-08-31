# Angular Standards

Standards for Angular front-ends (Angular 17+), typically paired with an ASP.NET Core Web API backend — the platform's default backend pattern for a client engagement that includes an Angular front end.

## Component design

- **Standalone components by default for new code.** Angular 17+ defaults to `standalone: true` components that declare their own `imports` rather than relying on an `NgModule`. If the target project's existing code is NgModule-based, match that instead — converting an established NgModule feature area to standalone is a scoped migration decision, not something to do incidentally while implementing an unrelated change.
- **Single responsibility per component**, the same discipline this platform applies to Blazor/Razor components. A component that renders a table, owns its own filtering, calls a service, and manages a modal is four components pretending to be one — split it.
- Type every `@Input()`/`@Output()` explicitly. `@Input({ required: true })` (Angular 16+) for anything not meaningfully optional turns a missing binding into a compile-time error instead of a runtime `undefined`. `@Output()` naming for two-way `[(x)]` binding support must exactly match `xChange` paired with `@Input() x` — this is load-bearing, not stylistic.
- Default new components to `ChangeDetectionStrategy.OnPush` in a project that already uses it elsewhere. It matters most for list/table-heavy components: `OnPush` only re-checks a component when an `@Input()` reference changes, an event originates from within the component itself, an `async`-piped observable emits, or a read signal changes — mutating an `@Input()` object's properties in place, rather than replacing the reference, silently stops triggering re-renders under `OnPush` and is a common source of "why isn't this updating" bugs.
- **Signals vs. RxJS for component state.** Signals (`signal()`, `computed()`, `effect()`) are Angular's newer reactive primitive and the default recommendation for new component-local state in a project not already committed to an RxJS-heavy pattern. Check the existing project's convention before introducing the other — running two competing reactivity models for the same kind of state is genuinely harder to reason about than either alone, and this decision should be made deliberately, not per-component.
- The service layer owns API calls and cross-component state; components consume services, they don't call `HttpClient` directly. A component reaching into `HttpClient` itself is a sign the service boundary was skipped.

## RxJS subscription-management discipline

- Prefer the `async` pipe (`{{ value$ | async }}`, `*ngIf="data$ | async as data"`) over manual `.subscribe()` wherever a value is only needed in the template — it subscribes on render and unsubscribes on destroy automatically, eliminating an entire class of leak.
- When a manual subscription is genuinely necessary, tie its lifetime to the component with `takeUntilDestroyed()` (Angular 16+, via `DestroyRef` or an injection context) or a `destroy$` `Subject` piped through `takeUntil(this.destroy$)` and completed in `ngOnDestroy`. An unmanaged `.subscribe()` with no teardown is the single most common Angular memory leak — it keeps the component, and anything its closure captures, alive after the component is destroyed and routed away from.
- Don't nest subscriptions to sequence async work; use `switchMap`/`mergeMap`/`concatMap`/`combineLatest` instead. A `.subscribe()` inside another `.subscribe()` is both a leak risk (the inner subscription isn't tied to the outer's lifecycle) and a sign the operator chain needs restructuring.
- Register `HttpInterceptor`s (`HTTP_INTERCEPTORS`, or the functional `HttpInterceptorFn` in Angular 15+) for auth-token attachment and centralized error handling instead of repeating header/error logic per service method.

## Forms strategy

- **Reactive Forms** (`FormGroup`/`FormControl`/`FormBuilder`) are the default for anything beyond a single trivial field — they're unit-testable without rendering the DOM and support strictly typed controls (Angular 14+ `FormControl<string>` rather than the historical loosely-typed API), catching a control-name typo or wrong value type at compile time instead of producing a silent `undefined` at runtime.
- **Template-driven Forms** (`ngModel`) are acceptable for a genuinely trivial form (one or two fields, no cross-field validation) — don't reach for them by default once a project has standardized on Reactive Forms elsewhere.
- Centralize custom validators as standalone, unit-testable `ValidatorFn`/`AsyncValidatorFn` functions rather than duplicating inline arrow functions across forms.

## Testing philosophy

- Jasmine + Karma (`ng test`) is Angular's traditional default; some projects have migrated to Jest. Check `angular.json`'s `test` builder and `package.json` before assuming which is in place, and never introduce the other into a project that has already standardized.
- Configure the component under test with `TestBed.configureTestingModule` — `imports: [MyComponent]` for a standalone component rather than `declarations`.
- Test through the rendered template and user-facing behavior: query the DOM for what a user would see or click, trigger interactions with a real `.click()` rather than calling a private method, and assert on rendered output or emitted `@Output()` values — not on internal fields a real consumer never touches.
- Mock `HttpClient` via `HttpClientTestingModule`/`HttpTestingController` (`expectOne(url)`, `.flush(mockResponse)`) rather than letting a test hit the network or hand-rolling a fake client.
- Call `fixture.detectChanges()` after any state change that should trigger a re-render before asserting on the DOM — asserting on markup captured before change detection ran is a common false-negative.

## Security notes

- Angular auto-escapes interpolation and property bindings (`{{ value }}`, `[innerText]`, `[value]`) by default — this is the safe path and needs no special handling.
- `DomSanitizer.bypassSecurityTrustHtml`/`bypassSecurityTrustUrl`/`bypassSecurityTrustResourceUrl`/`bypassSecurityTrustScript` disable that auto-escaping for whatever value is passed in. Treat any use of these methods on content that isn't fully trusted and server-validated as a genuine XSS risk — never call them on raw user-supplied or third-party content just to make a binding error disappear, and if one already exists in a project, verify what's actually flowing through it before touching surrounding code.
- `environment.ts`/`environment.prod.ts` are bundled into the client-side JavaScript at build time and are fully readable by anyone who opens the browser's dev tools. They're the right place for a public API base URL or a feature flag — never a real API key, client secret, or connection string. Anything genuinely secret stays server-side on the ASP.NET Core API, issued to the Angular client only as a short-lived token via an authenticated call.
- CORS is a backend-configuration concern (`AddCors`/`UseCors` on the ASP.NET Core API), not something an Angular app can fix client-side — a CORS failure surfaces in the browser console as a blocked request with no meaningful response body, and is easy to misdiagnose as a front-end bug when the fix belongs in the API's configuration.

## Related pages

- [Architecture Overview](Architecture-Overview.md)
- [Onboarding Guide](Onboarding-Guide.md)
