# Handle Client-Side Reconnection

**Category:** SignalR
**Use when:** users lose real-time updates silently after a network blip and the UI gives no indication anything went wrong.

## Prompt

Implement robust client-side reconnection handling for the SignalR connection in <describe the client, e.g., the JS/TS front end or .NET client>. Analyze the current HubConnection setup first: is withAutomaticReconnect() already configured, is there any UI feedback on connection state, and how does the app currently recover missed data after a reconnect? Propose the approach for my approval before implementing.

Requirements:
- Wire up onreconnecting(), onreconnected(), and onclose() handlers (or the equivalent Closed/Reconnecting/Reconnected events on the .NET client). onreconnecting should immediately switch the UI into a visibly degraded/"reconnecting" state (disable actions that require a live connection, show a banner/spinner) rather than failing silently.
- onreconnected() must not assume state is unchanged: the client gets a new ConnectionId after reconnecting, so any server-side per-connection state (group membership, presence) needs to be re-established here -- re-invoke join/subscribe methods as needed.
- onclose() (reconnect attempts exhausted or automatic reconnect disabled) must surface a clear, actionable error to the user (e.g., "connection lost, refresh to retry") and stop attempting silent operations against the dead connection.
- Address missed-message recovery: SignalR does not queue messages for a disconnected client, so design a reconciliation step on reconnect -- e.g., call a REST/hub method to fetch the current state or any messages/events since the last known sequence number/timestamp the client held, so gaps during the outage are backfilled instead of silently lost.
- Consider message ordering during the gap: if messages arrive concurrently with the backfill call, ensure the client can de-duplicate or order them (e.g., via a monotonically increasing sequence ID) instead of double-applying or misordering updates.
- If using automatic reconnect, review/tune the retry delay array against the app's real network conditions rather than accepting the default three quick retries then give-up.

Propose the reconnection/backfill design, implement it after approval, and write tests (or a documented manual test script if the client can't be unit tested) covering: reconnect after a transient drop, reconnect after group state needs restoring, and permanent disconnect (onclose) messaging.
