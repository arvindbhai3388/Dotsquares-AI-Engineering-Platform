# Add an HTTP Interceptor for Auth Token Attachment and 401 Handling

**Category:** Angular
**Use when:** API calls need a bearer token attached automatically, and/or the app needs a single place to react to token expiry (401) across every HTTP call.

## Prompt

Analyze how the app currently stores and reads the access/refresh token (a service backed by memory, `sessionStorage`, or an auth library) and how `HttpClient` is currently provided (`provideHttpClient(...)` in `app.config.ts`, or an older `HttpClientModule`), then propose the interceptor design — including whether token refresh will be a simple redirect-to-login on 401 or a proper silent-refresh-and-retry flow — before implementing. Refresh-and-retry adds real complexity (concurrent-request race handling); confirm which behavior is actually required rather than assuming the more complex one.

Implement it as a functional interceptor (`HttpInterceptorFn`), not the older class-based `HttpInterceptor`, unless the codebase already has class-based interceptors and consistency matters more here. Register it via `provideHttpClient(withInterceptors([authInterceptor]))`. Inside the interceptor, use `inject()` to get the token service/router — functional interceptors run in an injection context — and skip attaching the token to requests that clearly shouldn't carry it (the login/refresh endpoint itself, external URLs not pointing at this app's API) by checking the request URL rather than a hardcoded list that will silently rot.

Clone the outgoing request with `req.clone({ setHeaders: { Authorization: \`Bearer ${token}\` } })` rather than mutating `req` in place (`HttpRequest` is immutable by design). For 401 handling, use `catchError` on the forwarded `next(req)` call: on 401, either navigate to the login route via `Router` and clear the stored token, or — if a refresh flow was approved — call the refresh endpoint, and use a `BehaviorSubject`/shared `Observable` gate so concurrent 401s triggered by simultaneous in-flight requests don't each fire their own refresh call; only retry the original request(s) once a new token is available, and fail through to logout if the refresh itself fails.

Write tests using `provideHttpClientTesting()`/`HttpTestingController`: assert the `Authorization` header is present with the correct scheme on a normal request, assert it's absent on an excluded URL, and assert a simulated 401 response triggers the expected navigation/refresh-and-retry behavior. Cover the concurrent-401 case explicitly if refresh-and-retry was implemented — fire two requests, flush one 401, and assert only one refresh call was made.
