# Map User Identity to Connection IDs for Targeted Messaging

**Category:** SignalR
**Use when:** the server needs to push a message to a specific user regardless of which connection, tab, or device they're on.

## Prompt

Implement a mapping from user identity to one or more active SignalR ConnectionIds so the server can push targeted messages to a specific user (potentially connected from multiple tabs/devices simultaneously) via IHubContext, for <describe the use case, e.g., a notification service calling into the hub from outside>. Before implementing, check whether Clients.User(userId) (backed by the default or a custom IUserIdProvider) already satisfies this need without any custom mapping -- propose using the built-in mechanism first, and only build a custom ConnectionId map if there's a concrete reason the built-in one doesn't fit (e.g., needing to enumerate a user's active connections/devices explicitly, not just broadcast to all of them).

If a custom mapping is genuinely needed:
- Implement it as a singleton service registered in DI (not a static field), using a thread-safe structure (e.g., ConcurrentDictionary<string, HashSet<string>> guarded appropriately, or ConcurrentDictionary<string, ConcurrentBag<string>>) mapping user ID to the set of that user's active ConnectionIds, since a single user can have multiple simultaneous connections (multiple tabs/devices) and all must be tracked, not just the most recent.
- Populate it in OnConnectedAsync (add Context.ConnectionId under Context.UserIdentifier) and clean it up in OnDisconnectedAsync (remove that specific ConnectionId, and remove the user entry entirely once their connection set is empty) -- get this cleanup right or the map will leak entries for users who've long since disconnected.
- If the app scales out across multiple instances (backplane or Azure SignalR Service), this in-memory map is per-process and will be incomplete/wrong -- either confirm this feature only needs to work per-instance, or move the mapping to a shared store (Redis) keyed the same way, and prefer Clients.User(...)/Groups.AddToGroupAsync(connectionId, $"user:{userId}") instead, which are backplane-aware, over a hand-rolled in-memory map.
- Ensure the mapping is only ever populated from Context.UserIdentifier (server-resolved identity), never from a client-supplied user ID parameter, to prevent a client from registering itself as an arbitrary other user.
- Expose a method on the service (e.g., GetConnectionIds(userId)) for the IHubContext-based caller to use, rather than having external code reach into hub internals directly.

After approval, implement it, then write tests for: multiple simultaneous connections per user, cleanup on disconnect of one of several connections (the other must remain tracked), and cleanup when the last connection for a user disconnects.
