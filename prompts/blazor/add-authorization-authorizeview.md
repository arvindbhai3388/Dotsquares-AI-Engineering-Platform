# Add Role/Policy-Based UI with AuthorizeView

**Category:** Blazor
**Use when:** Parts of the UI should only render for users with certain roles or claims.

## Prompt

Before implementing, confirm the exact authorization requirement — a simple role check, a claim value check, or a named policy that should be registered centrally (`services.AddAuthorizationCore(options => options.AddPolicy(...))`) — and propose whether to use `<AuthorizeView Roles="...">`, `<AuthorizeView Policy="...">`, or a custom `IAuthorizationRequirement`/`AuthorizationHandler` if the rule is more complex than a role/claim match. Get my approval before implementing new policies, since policies are typically registered app-wide and affect more than this one component.

Wrap the conditional markup in `<AuthorizeView>` with the appropriate `Roles`/`Policy` attribute, and always provide both the `<Authorized>` and `<NotAuthorized>` (and optionally `<Authorizing>`) render fragments explicitly when the fallback UI matters — don't rely on the default empty fallback if hiding content silently would confuse users who expect some explanation or a call-to-action (e.g. a "sign in to continue" prompt). If the same authorization check gates multiple independent regions of a page, consider computing the boolean once via `AuthenticationStateProvider`/`ClaimsPrincipal` in code rather than repeating `<AuthorizeView>` blocks, to avoid redundant re-evaluation and markup duplication.

Remember `<AuthorizeView>` is a UI-only convenience — it must never be the only authorization boundary. Confirm the underlying data/action (API call, command handler) enforces the same authorization server-side independent of what the UI shows, since a user can trivially bypass client-rendered conditionals via dev tools or direct API calls. Call this out explicitly in your response if you find UI-only authorization protecting a sensitive action.

Handle the loading state: `AuthenticationStateProvider` resolves asynchronously, so a component that also fetches its own data in `OnInitializedAsync` should not assume authentication state is settled at that point — use `<AuthorizeView>`'s own `Authorizing` fragment or a `CascadingAuthenticationState` wrapper (already present at the app's layout root, if this app uses one) instead of racing a manual check. Add bUnit tests using `TestContext.AddTestAuthorization()` to set fake roles/policies and assert the `Authorized`/`NotAuthorized` fragment renders correctly under each identity configuration, including the anonymous/unauthenticated case.
