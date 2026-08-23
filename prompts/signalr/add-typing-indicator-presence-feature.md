# Add Typing Indicator / Presence Feature

**Category:** SignalR
**Use when:** building a collaborative or chat-like feature that needs live presence signals (who's online, who's typing).

## Prompt

Implement a typing-indicator and/or presence (who's online) feature for <describe the context, e.g., a chat room / a shared document editor> using SignalR groups and lightweight, non-persisted state. Before implementing, confirm the scope: is this presence for a single group/room, or global online-status across the app -- and check whether an existing group-management pattern (see the group-based-broadcast prompt) is already in place to build on rather than duplicating group-join logic.

Design and propose for approval:
- Model presence/typing state as ephemeral, in-memory data (this is not data that belongs in the primary database) -- e.g., a per-room ConcurrentDictionary<string, HashSet<string>> of currently-typing user IDs, or rely on group membership itself (Groups.AddToGroupAsync on join) as the source of truth for "who's online in this room," rather than introducing a persistent presence table unless the app genuinely needs durable presence history.
- A typing-indicator method (e.g., NotifyTyping(roomId)) that broadcasts to Clients.OthersInGroup(roomId) (not Clients.Group, to avoid echoing the typing signal back to the person who's typing) with the caller's identity, and expect the client to auto-clear the indicator after a short timeout (e.g., 3-5 seconds) client-side rather than requiring an explicit "stopped typing" call for every keystroke pause -- this keeps the feature resilient to a client that never sends a stop signal (e.g., due to a dropped connection).
- Throttle typing notifications from a single client (see the throttling prompt) since typing events can fire on every keystroke -- debounce/throttle server-side or require the client to debounce before invoking, and state which approach is used.
- Presence-on-connect/disconnect: update the "online in this room" set in OnConnectedAsync (after the client explicitly joins the room's group) and OnDisconnectedAsync, then broadcast the updated presence list (or a delta: "user X joined/left") to the group -- remember a single user may have multiple connections (multiple tabs), so track by ConnectionId within the room's set and only announce "user went offline" once their last connection in that room disconnects, not on every tab closing.
- Handle reconnection: after a client reconnects (new ConnectionId), it must re-join the room group and re-register presence -- treat a reconnect exactly like a fresh join for presence-tracking purposes, and ensure the old ConnectionId's presence entry gets cleaned up via the normal OnDisconnectedAsync path so stale "online" entries don't accumulate.
- If scaled out (backplane or Azure SignalR Service), avoid tracking presence in process-local memory -- either scope this feature to acceptable per-instance accuracy (documented as a known limitation) or back it with a shared store (Redis) keyed consistently with the group name.

After approval, implement it, then test: multiple users joining/leaving a room update presence correctly, a single user's typing indicator doesn't echo back to themselves, and a dropped/reconnected client's presence is cleaned up and correctly re-established.
