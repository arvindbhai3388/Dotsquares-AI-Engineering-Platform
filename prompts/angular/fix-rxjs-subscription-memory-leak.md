# Fix a Memory Leak from an Unmanaged RxJS Subscription

**Category:** Angular
**Use when:** A component subscribes to an Observable (a service stream, `valueChanges`, a `Router` event, a WebSocket) without ever unsubscribing, and instances keep doing work or holding memory after the component is destroyed.

## Prompt

Read the component fully and list every `.subscribe(...)` call, every `Observable` the component creates or holds a reference to (including ones assigned to a field but only used once), and every place it registers a DOM/window event listener or timer (`setInterval`, `addEventListener`) that has the same lifecycle problem. For each one, report whether it is already managed (piped through `takeUntilDestroyed()`, added to a `Subscription` bag that's unsubscribed in `ngOnDestroy`, or naturally completes on its own — e.g. an `HttpClient` call) or genuinely leaking, and don't propose a fix for subscriptions that already complete/unsubscribe correctly.

For each real leak, prefer this order of fixes and pick the one that fits the surrounding code style rather than defaulting to the same fix everywhere: (1) `takeUntilDestroyed()` from `@angular/core/rxjs-interop`, called either in a field initializer (injection context, no `destroyRef` argument needed) or passed an explicitly injected `DestroyRef` if subscribing later inside a method; (2) the `async` pipe in the template instead of a manual `.subscribe()` in the class, when the value is only ever displayed and not needed procedurally; (3) as a last resort for older patterns already dominant in this codebase, a `private readonly destroy$ = new Subject<void>()` piped with `takeUntil(this.destroy$)` and `this.destroy$.next(); this.destroy$.complete();` in `ngOnDestroy`. Do not introduce a fourth style if one of these three is already the codebase convention.

Watch specifically for the subtler leak shapes: a subscription created inside `ngOnInit` that re-subscribes on every `@Input()` change without disposing the previous one; a subscription to a long-lived singleton service's `Subject`/`BehaviorSubject` (these outlive the component by definition and are the most common real leak); and a nested `switchMap`/`mergeMap` inside a subscribe callback that itself needs cleanup.

Add or update a test that proves the fix: construct the fixture, trigger `fixture.destroy()`, and assert (via a spy on the source Observable's `subscribe`/via a manually completed test Subject) that no further emissions reach the component's callback after destruction. Confirm the test fails without your fix and passes with it.
