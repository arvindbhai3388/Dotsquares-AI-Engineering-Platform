# SignalR Guidelines

Guidance for designing and scaling SignalR hubs — the real-time transport underlying Blazor Server circuits (see [Blazor Standards](Coding-Standards-Blazor.md)) as well as any explicit real-time feature (live notifications, dashboards, chat) built on top of ASP.NET Core.

## Hub design

- A hub is a **thin RPC surface**, not a place for business logic — a hub method should validate input, delegate to an application/service-layer method, and broadcast the result. The same thin-controller discipline from [ASP.NET Core standards](Coding-Standards-AspNetCore-MVC-Razor.md) applies here.
- Keep hub method signatures small and specific (`Task SendMessage(string groupId, string text)`) rather than one generic `Task Send(string type, object payload)` dispatch method — the generic form loses compile-time safety and makes client-side typing (especially from a strongly-typed TypeScript/C# client) much harder to maintain.
- Use **strongly-typed hubs** (`Hub<TClient>` with an interface describing client-callable methods) so the compiler catches a mismatched method name/signature between server and client instead of failing silently at runtime with mistyped `Clients.All.SendAsync("methdoName", ...)` string-based calls.
- Hub instances are **transient** — a new instance is created per method invocation by design. Never store per-connection state in instance fields expecting it to persist between calls; use `Context.ConnectionId`-keyed external state (a scoped/singleton service, a cache, or the database) instead.
- Keep message payloads small. SignalR is for events/notifications and small state deltas, not for pushing large documents or file contents — use a regular HTTP endpoint (with a link/reference pushed over SignalR) for anything beyond a modest payload size.

## Groups vs. users

- **`Clients.User(userId)`** targets all of a specific user's active connections (accounting for the same user having multiple tabs/devices open) — requires a configured `IUserIdProvider` (default maps to `ClaimTypes.NameIdentifier`) so SignalR can resolve a connection to a stable user identity. Use this for "notify this specific person" scenarios (their own notifications, their own document's status).
- **`Clients.Group(groupName)`** targets an arbitrary named set of connections a client explicitly joined via `Groups.AddToGroupAsync(Context.ConnectionId, groupName)`. Use this for shared-context broadcast — everyone viewing a specific document, everyone in a specific chat channel, everyone on a specific dashboard.
- Group membership is **connection-scoped, not user-scoped, and not persisted** — it lives only in SignalR's in-memory (or backplane-shared) group tracking and must be re-established in `OnConnectedAsync` (or on-demand when the client navigates to that context) every time a new connection is made; a reconnect after a network blip starts with no group memberships.
- Authorize group joins server-side inside the hub method that adds the connection to the group (verify the caller is actually allowed to see that document/channel) — never trust a client-supplied group name at face value without checking authorization, since group names are effectively just strings the client can request to join.
- Prefer groups over "loop through every user and check a permission" broadcast patterns — group membership is the mechanism that scales; re-evaluating authorization for every connected client on every message does not.

## Scaling with a backplane

A single SignalR server instance keeps its connections, groups, and user mappings entirely in memory. The moment there is more than one server instance (a scaled-out App Service, multiple Kubernetes pods, or on-prem load-balanced IIS instances), a client connected to instance A cannot receive a message triggered by logic running on instance B unless a **backplane** relays it.

- **Azure SignalR Service** is the default recommendation for anything hosted in Azure — it moves the actual client connections off the app servers entirely (clients connect to the Azure SignalR Service, which relays to/from the app server over a separate connection), which also solves the "sticky sessions" problem a plain load balancer would otherwise need to handle for WebSocket connections. It scales connection count independently of app server scale and requires only an SDK package + connection string, no infrastructure to run.
- **Redis backplane** (`Microsoft.AspNetCore.SignalR.StackExchangeRedis`) is the right choice for on-prem/non-Azure hosting, or where a Redis instance already exists in the client's infrastructure for other purposes. Unlike Azure SignalR Service, app servers still hold the actual client connections directly — Redis only relays messages between server instances — so sticky sessions (or a load balancer capable of them) are still required at the load balancer.
- Without a backplane, do not scale a SignalR-dependent feature (or Blazor Server app) past a single instance — a client whose hub method call needs to reach a different server instance's connections will silently fail to notify those clients, a bug that will not show up in single-instance local development or testing.
- For Blazor Server apps specifically: this applies to the app's own SignalR circuit, not just custom hubs — a Blazor Server app scaled out without a backplane (or without disabling scale-out) will drop users' interactivity unpredictably when a load balancer routes a request to a different instance than the one holding their circuit.

## Authentication and authorization on hubs

- Apply `[Authorize]` at the hub class level for any hub carrying non-public data — SignalR respects the same ASP.NET Core authentication/authorization pipeline as controllers, including policy-based `[Authorize(Policy = "...")]`.
- The hub negotiation/connection request carries the auth cookie/bearer token the same way an HTTP request would, **except** for WebSocket/Server-Sent Events transport fallback scenarios in browser clients where a bearer token must instead be supplied via the `access_token` query string parameter (SignalR's client SDK handles this automatically when configured with `.AccessTokenProvider`) — be aware this puts the token in the URL for that specific negotiation request, which is a deliberate, documented SignalR pattern but still worth being conscious of in logging configuration (avoid logging full request URLs for hub negotiation endpoints).
- Re-check authorization **inside** hub methods for anything beyond "is this user allowed to connect at all" — `[Authorize]` on the hub proves the caller is authenticated, not that they're allowed to join a specific group or act on a specific resource; that's an object-level authorization check the method itself must perform (e.g., does this user have access to *this* document's group).
- `Context.User` inside a hub reflects the claims principal established at connection time — if a user's roles/permissions change while they have an active connection, that change is not reflected in `Context.User` for the current connection until they reconnect, which matters for any feature that gates access to a fast-changing permission set.
- Never rely on obfuscated group names or connection IDs as a substitute for real authorization — a group name being hard-to-guess is not the same thing as a group being access-controlled.

## Related pages

- [Blazor Standards](Coding-Standards-Blazor.md) — SignalR as Blazor Server's transport.
- [Architecture Overview](Architecture-Overview.md)
- [Security Guidelines](../docs/Security-Guidelines.md)
