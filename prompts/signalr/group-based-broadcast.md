# Implement Group-Based Broadcasting

**Category:** SignalR
**Use when:** broadcasting to all connected clients is too broad and messages should only reach a specific subset (chat room, tenant, document, etc.).

## Prompt

Implement group-based broadcasting in the specified Hub so that messages for <describe the scope, e.g., a chat room / tenant / document> are only delivered to clients who have joined the corresponding SignalR group, instead of using Clients.All. Start by analyzing the current broadcast mechanism, the group-naming scheme already used elsewhere in the codebase (if any), and how clients currently join/leave -- then propose the group-key naming convention and join/leave lifecycle for my approval before implementing.

Cover these specifics:
- Add explicit JoinGroup/LeaveGroup hub methods (or fold group management into OnConnectedAsync/OnDisconnectedAsync where appropriate) using Groups.AddToGroupAsync and Groups.RemoveFromGroupAsync. Never assume a client that connects is automatically in the right group -- require an explicit, authorized join call.
- Authorize the join: verify the caller is actually permitted to join the requested group (e.g., is a member of that tenant/room/document) before adding them, not after.
- Use a collision-resistant, deterministic group name (e.g., prefix + resource ID) and centralize the naming logic in one helper so callers can't drift into typos that silently create the wrong group.
- Handle disconnects and reconnects: SignalR does not automatically restore group membership after a dropped connection gets a new ConnectionId, so ensure OnConnectedAsync (or client-side reconnect logic) re-joins the correct groups on every reconnect, and OnDisconnectedAsync cleans up any per-connection tracking (but note groups themselves need no explicit removal on disconnect since SignalR does this automatically).
- If running with a backplane (Redis/SQL) or Azure SignalR Service, confirm group broadcasts (Clients.Group(...)) are routed correctly across all server instances -- this is native behavior for both, but verify the specific backplane/provider is actually wired up rather than assuming it.
- Avoid leaking group membership or broadcasting to a group the caller isn't part of; do not expose raw group names to unauthorized clients.

Propose the design, get my approval, implement it, then add tests for join authorization, correct scoping of Clients.Group delivery (a client outside the group must not receive the message), and rejoin-after-reconnect behavior.
