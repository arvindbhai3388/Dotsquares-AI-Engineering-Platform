# Migrate Classic ASP.NET SignalR to ASP.NET Core SignalR

**Category:** SignalR
**Use when:** modernizing a legacy .NET Framework SignalR implementation.

## Prompt

Plan a migration of the specified legacy ASP.NET (classic, Microsoft.AspNet.SignalR / OWIN-hosted) SignalR implementation to ASP.NET Core SignalR. This is a significant architectural change -- produce and get approval on a full plan before touching any code. Start by inventorying what's actually in use: PersistentConnection-based endpoints versus Hub-based endpoints, any direct GlobalHost.ConnectionManager usage, the OWIN Startup configuration, client libraries in use (jquery.signalr, or the old .NET client), and any custom IPersistentConnectionContext / IConnectionIdFactory customizations that have no direct ASP.NET Core equivalent.

The plan must map old concepts to new ones explicitly:
- PersistentConnection (classic) has no direct ASP.NET Core SignalR equivalent -- these must be redesigned as Hub-based endpoints, since ASP.NET Core SignalR is Hub-only; identify every PersistentConnection subclass and plan its Hub-shaped replacement method-by-method rather than assuming a mechanical port.
- GlobalHost.ConnectionManager.GetHubContext<THub>() (classic, used to push messages from outside a hub, e.g., a controller) becomes constructor-injected IHubContext<THub> (or IHubContext<THub, TClient> for strongly-typed clients) via DI in ASP.NET Core -- every call site needs updating, not just the hub classes themselves.
- OWIN's app.MapSignalR() in Startup.cs becomes endpoints.MapHub<THub>("/path") inside UseEndpoints in ASP.NET Core's Program.cs/Startup.cs, and CORS, authentication, and any OWIN middleware the classic app relied on need ASP.NET Core equivalents configured explicitly -- do not assume OWIN middleware carries over.
- The old JSON serializer configuration (classic SignalR used Newtonsoft.Json by default) may need explicit AddNewtonsoftJson() configuration in ASP.NET Core SignalR if any client depends on Newtonsoft-specific serialization behavior (e.g., $type handling, date formats) that differs from the new System.Text.Json default.
- Client-side: the old jquery.signalr client is incompatible with ASP.NET Core SignalR's protocol -- every client (JS, or old .NET HubConnection from Microsoft.AspNet.SignalR.Client) must be upgraded to @microsoft/signalr or Microsoft.AspNetCore.SignalR.Client, and connection-building code (`$.connection.hub.start()` etc.) needs a full rewrite, not a config tweak.
- Group and connection-ID persistence: classic SignalR's connection/group state and this app's equivalents must be re-verified against ASP.NET Core's Groups/Context.ConnectionId APIs, since internal representations differ even though the surface API is similar.
- Plan a coexistence or big-bang cutover strategy given clients cannot speak both protocols simultaneously against the same endpoint, and identify a rollback path.

Present the full plan with a risk/effort breakdown per hub/endpoint for approval. Only after approval, implement incrementally (one hub/endpoint at a time), validating each with tests before moving to the next, and report status clearly against the original inventory.
