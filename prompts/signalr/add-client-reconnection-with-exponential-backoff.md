# Configure Client Reconnection with Exponential Backoff

**Category:** SignalR
**Use when:** the default reconnect policy isn't appropriate for the app's actual network conditions.

## Prompt

Configure a custom exponential backoff retry policy on the HubConnectionBuilder for <describe the client, e.g., the mobile app / the JS front end operating on flaky networks>, replacing the default withAutomaticReconnect() behavior (which retries at 0s, 2s, 10s, 30s and then gives up) with a policy tuned to this app's actual conditions. Before implementing, clarify what "appropriate" means here: is the goal longer sustained retrying (e.g., mobile clients on intermittent connectivity that shouldn't give up after 30 seconds), faster initial retry for a low-latency requirement, or a capped maximum backoff to avoid hammering the server -- propose the specific curve for approval before writing code.

Implementation requirements:
- Implement IRetryPolicy (NextRetryDelay(RetryContext retryContext)) rather than relying on the fixed default array, computing an exponential delay (e.g., base * 2^attempt) capped at a sane maximum (e.g., never wait longer than 60s between attempts) and optionally with jitter (randomized +/- percentage) added to avoid a thundering-herd reconnect storm if many clients drop simultaneously (e.g., after a server restart) and would otherwise all retry in lockstep.
- Use retryContext.PreviousRetryCount and retryContext.ElapsedTime to decide whether to keep retrying or return null (which stops automatic reconnection and moves the connection to Disconnected, triggering onclose) -- decide and document the give-up condition (e.g., stop after N attempts or after M total minutes) rather than retrying forever silently.
- Pass the custom policy via .withAutomaticReconnect(customRetryPolicy) (JS) or .WithAutomaticReconnect(customRetryPolicy) (.NET client) and confirm it's wired to the same onreconnecting/onreconnected/onclose handlers already established (see the reconnection-handling prompt) so backoff changes don't silently break the UI feedback loop.
- Consider that retries happen with the same HubConnection instance, so state that must be re-established after a successful reconnect (group membership, missed-message backfill) still needs to run in onreconnected -- backoff timing changes do not change that requirement.
- If the app runs behind Azure SignalR Service or a backplane, confirm the backoff policy doesn't conflict with any server-side connection limits or negotiate-endpoint rate limits during a reconnect storm.

After approval, implement the custom policy, then test it by simulating a dropped connection (e.g., stopping the server or blocking the network in a controlled test/dev environment) and confirming the actual delay sequence matches the intended curve and that give-up behavior triggers onclose correctly.
