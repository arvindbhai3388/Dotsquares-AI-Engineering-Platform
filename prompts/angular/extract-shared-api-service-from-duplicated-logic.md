# Extract a Shared Service from Duplicated API-Call Logic

**Category:** Angular
**Use when:** The same (or near-same) `HttpClient` call, error handling, or response mapping is copy-pasted across multiple components.

## Prompt

Find every component with a copy of this logic and report the actual differences between the copies — not just the similarities — before proposing the extracted service's shape: differing base URLs/endpoints, differing query parameters, differing response shapes needing different mapping, differing error-handling behavior (one silently swallows errors, another shows a toast), and differing caching/sharing needs. Propose the service's public method signatures, injection scope (`providedIn: 'root'` for a true app-wide singleton vs. scoped to a feature), and how the differences above will be parameterized rather than hardcoded per-caller, and wait for approval before touching the components.

Implement the service with `inject(HttpClient)` and return typed `Observable<T>` from each method — define or reuse a real interface/type for the response shape rather than letting callers deal with `any` or an inline object type repeated at each call site. Move shared response mapping into the service via `.pipe(map(...))` so callers get already-shaped data, and centralize shared error handling with `catchError` only if every caller actually wants the same behavior — if callers currently handle errors differently, expose the raw error via `catchError(err => throwError(() => err))` (or a typed error wrapper) and let each caller decide, rather than forcing one behavior on all of them.

If multiple components call the same endpoint with the same parameters roughly simultaneously (e.g. several widgets in one page fetching the same "current user" data), consider sharing the in-flight/most-recent response via `shareReplay({ bufferSize: 1, refCount: true })` on a memoized `Observable`, but only add this caching if there's a real duplicate-call cost — don't cache calls that already differ in parameters per caller.

Update each component to inject the new service and remove its local copy of the HTTP logic, but do not change what each component does with the result beyond what's needed to fit the new service's return type (e.g. don't fix unrelated bugs in one caller while extracting). Write unit tests for the new service using `HttpTestingController` covering the success path, the error path, and (if implemented) that a shared/cached call only hits the backend once for concurrent subscribers. Update or add tests for each component confirming it still renders correctly against a mocked service (via a stub/spy, not a real `HttpClient`).
