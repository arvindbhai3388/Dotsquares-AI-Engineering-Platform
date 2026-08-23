# Convert to a Strongly-Typed Hub Client

**Category:** SignalR
**Use when:** client method names are stringly-typed (SendAsync with magic strings) and error-prone to maintain.

## Prompt

Convert the specified Hub from using string-based client invocation (e.g., Clients.Caller.SendAsync("MethodName", arg1, arg2)) to a strongly-typed Hub<TClient> using an interface that declares each client-callable method with its real parameter types. Before changing anything, locate every SendAsync/InvokeAsync call site targeting this hub's clients (including from IHubContext<THub> usages outside the hub itself, e.g., in controllers or background services) so the interface covers every method actually invoked, then propose the interface shape for my approval.

Implementation requirements:
- Define an interface (e.g., IChatClient) with one method per distinct client-side handler currently referenced by string, matching the exact parameter types and order the JS/TS or .NET client already expects -- a mismatch here is a silent runtime break, not a compile error, since the client side isn't type-checked against this interface.
- Change the Hub's base class from Hub to Hub<TClient> and replace every Clients.X.SendAsync("Method", args) call with Clients.X.Method(args), removing the magic strings entirely.
- Update every external IHubContext<Hub> injection site (controllers, services, background workers) to IHubContext<Hub, TClient> and update their calls the same way.
- Verify method names match the client-side handler registration exactly (e.g., connection.on("MethodName", ...) in JS) -- strongly-typing the server side does not protect against a client-side name mismatch, so cross-check both ends and flag any mismatch found during the audit instead of silently "fixing" the client without approval.
- Preserve backward compatibility: if any external/older client still depends on the exact wire method name, confirm the strongly-typed interface serializes with the same method name (it does by default, using the interface method name), and note any case where a client used a different casing than the C# method name.
- Do not change unrelated hub methods, business logic, or the actual message payloads/DTOs -- this is a mechanical type-safety refactor, not a behavior change.

After approval, implement the interface and refactor, then build and run any existing hub-related tests to confirm no call sites were missed (a missed SendAsync will be a compile error under Hub<TClient>, which is the point), and report the full list of call sites updated.
