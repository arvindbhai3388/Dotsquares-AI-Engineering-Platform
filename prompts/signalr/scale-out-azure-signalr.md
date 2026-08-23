# Plan Scale-Out to Azure SignalR Service

**Category:** SignalR
**Use when:** the app needs to run on multiple instances or scale out horizontally and is currently running SignalR in-process.

## Prompt

Plan (and, once approved, implement) migrating the specified Hub(s) from in-process ASP.NET Core SignalR to Azure SignalR Service so the app can scale out across multiple instances. This is an architectural change -- do not implement anything until the plan is approved. First analyze the current hosting setup: how many hubs exist, whether any code assumes in-process/single-instance behavior (e.g., static in-memory dictionaries for connection tracking, IHubContext usage, sticky-session assumptions), and what the current deployment topology is (App Service, VM, container, load balancer config).

The plan must address:
- Adding the Microsoft.Azure.SignalR NuGet package and wiring services.AddSignalR().AddAzureSignalR(...) with the connection string sourced from configuration/Key Vault (do not hardcode or ask me to paste the connection string into chat -- reference the config key only).
- Removing any reliance on sticky sessions / ARR affinity at the load balancer, since Azure SignalR Service handles client-to-server routing itself and in-process affinity assumptions become invalid or unnecessary.
- Auditing and removing any static, per-process, in-memory state used for connection tracking, presence, or group bookkeeping (e.g., a ConcurrentDictionary<string, string> mapping users to ConnectionIds) -- this must move to a shared store (the service itself, or Redis/SQL) because it will no longer be consistent across instances or even meaningful once Azure SignalR Service brokers connections.
- Confirming all Clients.Group/Clients.User/IHubContext broadcasts still work correctly when connections are held by Azure SignalR Service rather than the app server directly -- these APIs are unchanged in usage but now route through the service.
- Reviewing message size and connection count limits/pricing tier implications for Azure SignalR Service versus current usage.
- Planning the cutover: whether this can be a rolling deployment (Default mode) or requires Serverless/Classic mode considerations, and how to validate the new path in a staging environment before production cutover, including a rollback plan.
- Updating client-side connection strings/negotiate endpoints if the client currently hardcodes a hub URL assuming direct connection.

Present the plan with a risk list and rollback strategy for my approval. After approval, implement incrementally, validate in a non-production environment, and report exactly what was tested (connection, group broadcast, reconnection under the new topology) versus what still needs manual verification in the target Azure environment.
