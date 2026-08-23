# Starter Scaffold — SignalR

> Template outline for bootstrapping SignalR real-time functionality inside an ASP.NET Core
> project. This is a folder-structure and setup guide, not a working demo — see `demos/` for
> a runnable example. SignalR is normally added into an existing ASP.NET Core/Blazor Server
> project rather than standing alone — structure below assumes that.

## Recommended Folder Structure

```text
<ExistingProjectName>/
├── Hubs/
│   ├── <Feature>Hub.cs               # One hub per cohesive real-time feature, not one god-hub
│   └── I<Feature>Client.cs           # Strongly typed client interface (Hub<I<Feature>Client>)
├── Services/
│   └── <Feature>NotificationService.cs  # Server-side code that pushes via IHubContext<T>, decoupled from the hub itself
├── Models/
│   └── <Feature>Message.cs           # DTOs sent over the wire — keep small and serializable
└── wwwroot/js/
    └── <feature>-hub-client.js       # Client-side connection setup, if not using a typed JS/TS client
```

## Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.AspNetCore.SignalR` (built into ASP.NET Core shared framework) | Core SignalR |
| `Microsoft.AspNetCore.SignalR.Client` | .NET client (for server-to-server or desktop/MAUI clients) |
| `Microsoft.Azure.SignalR` | Scale-out via Azure SignalR Service — needed once running more than one server instance |

## First Things to Configure

1. Use a **strongly typed hub** (`Hub<IClientInterface>`) instead of `Hub` with
   stringly-typed `Clients.Caller.SendAsync("MethodName", ...)` calls — compile-time safety
   on the client contract.
2. Push server-initiated messages via `IHubContext<THub, TClient>` injected into a regular
   service — don't route unrelated business logic through the hub class itself.
3. Decide the scale-out story up front if the app will run more than one server instance:
   Azure SignalR Service or a Redis backplane — a hub with in-memory group state breaks
   silently across multiple instances without one.
4. Authenticate hub connections the same way as the rest of the app (`[Authorize]` on the
   hub, and re-validate the user's permissions before joining any group, not just at
   connection time).
5. Set reconnection behavior explicitly on the client (`withAutomaticReconnect()` or
   equivalent) rather than leaving users silently disconnected.
6. Set up the paired test project — unit test hub methods by testing the injected services
   directly (mock `IHubContext`/`IClientProxy`) rather than standing up a real connection for
   every test (Test-First).
