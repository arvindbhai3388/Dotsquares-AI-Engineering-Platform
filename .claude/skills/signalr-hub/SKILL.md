---
name: signalr-hub
description: >
  Use when adding a new SignalR hub or a new method to an existing hub —
  covers auth, groups, and client contract versioning so the addition is
  safe for already-connected clients and multi-instance deployments.
  Trigger phrases: "add a SignalR hub", "add a method to this hub",
  "broadcast this event to clients". For general fixes to existing
  SignalR code, prefer the signalr-developer agent; use this skill when
  adding new hub surface specifically.
---

# SignalR Hub Addition Workflow

*Can be approved with a single Yes/No that carries this through to completion instead of a
per-step check-in — see `wiki/AI-Workflow-Discipline.md`'s "Streamlined mode" section.*

Adding hub surface is a public-contract change the moment any client
connects to it — this skill treats it that way from the start, rather
than as "just adding a method."

## Step 1 — Decide: new hub, or new method on an existing hub?

- Prefer adding a method to an existing hub when the new capability is
  in the same feature area (same group/connection lifecycle concerns) —
  most projects should have very few hubs.
- Create a new hub only when the capability has genuinely distinct
  connection/group/authorization semantics from existing hubs (a
  different audience, a different auth policy, a different scaling
  concern).
- Check existing hubs first (`Grep` for `: Hub` /`: Hub<T>`) before
  assuming none fits.

## Step 2 — Design the contract

- Define the strongly-typed client interface addition
  (`Task ReceiveXyz(ArgType arg)` on the shared `IXyzClient` interface
  used by `Hub<IXyzClient>`) rather than a stringly-typed
  `Clients.X.SendAsync("MethodName", ...)` call — this catches signature
  mismatches at compile time on both hub and any .NET client.
- Define the server-side method's parameters and return type
  deliberately — this is what JS/.NET clients will call; treat it as an
  API signature, not an internal implementation detail.
- Decide group scoping up front: does this broadcast to `Clients.All`,
  a `Clients.Group(name)`, or `Clients.User(userId)` (requires a
  configured `IUserIdProvider`)? Broadcasting wider than necessary is
  both a performance and (if the payload has any sensitivity) an
  authorization concern.

## Step 3 — Apply authorization

- Add `[Authorize]` at the hub class level for any hub requiring
  authentication, or on the specific new method if the hub is otherwise
  mixed-access.
- For resource-scoped actions (posting to a specific room/group the
  caller may or may not have rights to), check authorization to *that
  specific resource* inside the method body — `[Authorize]` alone only
  confirms the caller is authenticated, not that they're allowed to act
  on this particular group/resource.
- If the hub uses cookie-based auth (typical for browser clients), verify
  the negotiate/connect request actually carries the auth cookie
  (same-site/CORS settings) — a hub that silently rejects connections due
  to a CORS/cookie misconfiguration is a common integration failure to
  check for explicitly during testing, not just at code-review time.

## Step 4 — Implement group membership correctly

- Add connections to groups in `OnConnectedAsync` (and re-add in
  `OnReconnectedAsync` if the project handles reconnection explicitly) —
  group membership is per-**connection**, not per-user, and does not
  survive a reconnect automatically.
- If a user can have multiple simultaneous connections (multiple tabs/
  devices), ensure the join logic runs for each connection independently
  rather than assuming "the user" is now in the group.
- Keep hub methods thin — call an injected service for any real logic,
  don't embed business logic directly in the hub method (this also makes
  that logic unit-testable without a live hub/connection).

## Step 5 — Verify multi-instance/backplane safety

- If the target deployment runs (or might run) more than one server
  instance, confirm a backplane (Redis backplane, or Azure SignalR
  Service) is actually configured — a new hub method that broadcasts via
  `Clients.Group`/`Clients.All` will silently only reach clients on the
  same instance without one. This is a deployment-configuration check,
  not just a code review point — verify it against the actual
  target environment's configuration, don't assume.

## Step 6 — Version the client contract deliberately

- **Additive is safe**: adding a new hub method, or a new optional field
  appended to an existing message's payload (if using an object payload
  clients deserialize permissively), doesn't break clients still on the
  old contract — they simply don't call/use the new surface.
- **Breaking changes** (changing an existing method's parameter types/
  count, removing a method, changing a payload's existing field
  meaning/type) break any client still connected with the old contract
  assumption. Before making one:
  - Confirm whether old clients can be forced to reconnect/upgrade
    atomically with the server deploy (rare in a real rollout), or
  - Add the new behavior as a **new** method/event name alongside the
    old one, deprecate the old one, and remove it only after confirming
    no client still depends on it (mirrors expand/contract for schema
    changes — see efcore-migration).
- Document the contract change (hub interface, expected payload shapes)
  in the project's existing documentation location so client-side
  developers (including other teams) aren't discovering it from source.

## Step 7 — Test and validate

- Unit-test any logic extracted into a service the hub method calls,
  using the project's detected test framework (see unit-testing skill).
- If integration-testing the hub itself, use a real
  `HubConnection`/`TestServer` pairing rather than only unit-testing the
  hub class in isolation, since group/auth/connection behavior is hard to
  fake convincingly with a bare mock.
- Run `build-validator` (or the project's own build/test commands) before
  calling the addition done.

## Do
- Design the client contract deliberately, as a real API.
- Apply and verify resource-scoped authorization inside the method, not
  just `[Authorize]` at the class level.
- Confirm backplane configuration for any multi-instance deployment
  target.
- Add new methods/events for breaking contract changes rather than
  mutating existing ones in place.

## Don't
- Don't store per-connection state as hub instance fields.
- Don't assume group membership survives reconnect.
- Don't change an existing hub method's signature without a
  deprecation/versioning plan for connected clients.
- Don't claim multi-instance broadcast works without verifying backplane
  configuration.
