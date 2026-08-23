# Add a New Hub Method

**Category:** SignalR
**Use when:** a new real-time action needs to be exposed to clients.

## Prompt

Add a new method to the specified SignalR Hub that exposes <describe the action> to connected clients. Before writing code, follow the analyze -> propose -> approve -> implement -> test -> review workflow: first locate the Hub class, its base type (Hub or Hub<TClient>), existing method conventions, and how the caller invokes it (HubConnection.InvokeAsync/SendAsync), then propose the method signature and behavior for my approval before implementing.

The new method must:
- Validate all incoming parameters explicitly (null checks, range checks, string length limits) before touching any state or external resource. Do not trust Context.User or Context.ConnectionId blindly if the operation is user-scoped -- confirm the caller is authorized to perform the action on the target resource, not just authenticated.
- Return errors to the caller in a structured, catchable way. If the method returns a value, throw a HubException (not a generic Exception) with a client-safe message so exception details are not leaked to the client (do not disable EnableDetailedErrors reliance for this). If the method is fire-and-forget, push a dedicated error/ack message back to the caller via Clients.Caller instead of letting the exception disappear.
- Consider message ordering: if this method's effects can race with other messages from the same or other clients (e.g., concurrent updates to shared state), document or enforce the ordering guarantee.
- Consider reconnection: if the client can legitimately retry this call after a dropped connection, make the operation idempotent or explain why it doesn't need to be.
- If the Hub is registered behind a backplane or Azure SignalR Service, confirm the method's side effects (e.g., group membership changes, IHubContext broadcasts) will propagate correctly across all server instances.
- Follow the existing DI, logging, and async/await patterns already used in sibling Hub methods.

After implementing, write or update a unit/integration test covering the success path, a validation-failure path, and an authorization-failure path, then report what was and wasn't verified.
