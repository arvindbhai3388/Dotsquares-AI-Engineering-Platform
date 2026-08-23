# Add Authentication/Authorization to a Hub

**Category:** SignalR
**Use when:** a hub currently accepts connections or invokes methods without verifying identity or permissions.

## Prompt

Add authentication and authorization to the specified SignalR Hub, which currently <describe the gap, e.g., accepts any connection / does not check per-method permissions>. Begin by analyzing how the rest of the application authenticates (cookie auth, JWT bearer, Windows auth) and how authorization policies are already defined (e.g., [Authorize] usage on controllers, existing policy names in Startup/Program.cs), so the hub follows the same scheme rather than inventing a parallel one. Propose the specific approach before implementing.

Cover:
- Apply [Authorize] at the Hub class level for baseline authentication, and add method-level [Authorize(Policy = "...")] or [Authorize(Roles = "...")] attributes where individual methods need stricter checks than the class-level default (e.g., an admin-only broadcast method on an otherwise user-accessible hub).
- Confirm the access token can actually reach the hub: for browser/JS clients using WebSockets, bearer tokens aren't sent in headers on the transport by default, so verify (or add) the OnMessageReceived JWT event handling that reads the token from the access_token query string specifically for requests to the hub path.
- For per-connection identity, do not trust any client-supplied user identifier -- always resolve identity from Context.User (populated by the authentication middleware), never from a method parameter the caller controls.
- For resource-level authorization (e.g., "can this user act on this specific group/document/tenant"), add an explicit check inside the method body (or a custom IHubFilter, see the hub-filter prompt) rather than relying solely on role-based [Authorize], since SignalR authorization attributes don't understand your domain's object-level permissions.
- Handle the unauthorized paths: an unauthenticated connection attempt should be rejected at connect time (resulting in a client-side connection failure) rather than allowed to connect and then silently failing every method call; a method-level authorization failure should throw HubException with a client-safe message, not leak stack traces.
- If group membership or Clients.User(...) targeting depends on identity, confirm the app has configured a stable IUserIdProvider (default is ClaimTypes.NameIdentifier) appropriate for this hub's user model.

After approval, implement the changes, then write tests covering: anonymous connection rejection, authenticated-but-unauthorized method calls, and authorized success, using the project's existing test/auth-mocking conventions.
