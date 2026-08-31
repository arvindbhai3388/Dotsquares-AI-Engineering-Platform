# Add a Custom RxJS Pipeable Operator for a Repeated Stream Transformation

**Category:** Angular
**Use when:** The same sequence of RxJS operators (a particular `retry`+`catchError` combination, a repeated mapping/filtering chain, a shared loading-state toggle around an HTTP call) is copy-pasted across multiple `.pipe(...)` calls.

## Prompt

Find every place the transformation is duplicated and confirm the operator sequence is actually identical (or identical modulo a parameter) before extracting it — a custom operator that has to branch internally for each caller's slightly different needs is usually worse than the duplication it replaces. Propose the operator's name, its generic type signature, and any parameters it needs, and wait for approval before implementing.

Implement it as a plain function returning a `MonoTypeOperatorFunction<T>` (if it doesn't change the emitted type) or `OperatorFunction<T, R>` (if it maps to a different type), following the standard pipeable-operator shape: `function withRetryAndToast<T>(label: string): MonoTypeOperatorFunction<T> { return (source: Observable<T>) => source.pipe(retry({ count: 2, delay: 500 }), catchError(err => { /* ... */ return throwError(() => err); })); }`. Do not implement it as a class extending `Operator`/using `lift()` — that low-level API is unnecessary for a composed transformation and is what the standard library itself has moved away from for user-defined operators.

If the operator needs a dependency that isn't itself part of the stream (a `ToastService` to show an error, a `LoggerService`), pass it in as a parameter to the factory function rather than calling `inject()` inside the operator body — an operator function runs outside Angular's injection context when the pipe executes, so `inject()` there will fail or silently resolve nothing depending on when it's called; require the caller (who is in injection context) to pass the already-injected dependency in.

Place the operator in a shared, discoverable location matching this codebase's convention for cross-cutting utilities (e.g. alongside other custom operators if any exist, or a new `rxjs-operators` file under a shared/core folder) rather than defining it locally in whichever component happened to need it first, and export it for reuse. Update each duplicated call site to use `source$.pipe(withRetryAndToast('save-profile'))` in place of the repeated inline chain, without changing any other behavior at those call sites.

Write unit tests for the operator in isolation using RxJS marble testing (`TestScheduler`) or a plain synchronous/`fakeAsync` test with a manually constructed source `Observable`/`Subject`: assert it retries the configured number of times on error before giving up, that a successful emission passes through unchanged, and that the injected dependency (toast/logger) is called with the expected arguments on final failure. Confirm each updated call site's existing tests still pass unchanged, since the operator should be a transparent refactor of behavior that already existed.
