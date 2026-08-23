# Demo 1 — ASP.NET Core Web API + EF Core Code-First + SignalR

A small **Project Task Tracker** API demonstrating this framework's supported stack for
API + data access work: **ASP.NET Core 8 Web API**, **EF Core 8 Code-First** against
**SQL Server / LocalDB**, and **SignalR** for real-time notifications.

## What this demonstrates

- REST CRUD over two related entities (`Project` 1—many `TaskItem`), with data-annotation
  validation, `ProblemDetails` error responses, and pagination on list endpoints.
- EF Core Code-First: a hand-written `DbContext`, an initial migration generated with
  `dotnet ef`, and a connection string read from configuration — never hardcoded.
- A thin controller → service → `DbContext` layering, so business logic (in
  `Services/ProjectService.cs` and `Services/TaskItemService.cs`) is unit-testable in
  isolation from ASP.NET Core.
- A SignalR hub (`Hubs/TaskHub.cs`) that broadcasts a `TaskStatusChanged` event to every
  connected client whenever a task's status changes via `PATCH /api/tasks/{id}/status` —
  the standard "Web API mutates state, SignalR fans the change out to live clients" pattern.
- The options pattern (`Options/PaginationOptions.cs`) for a configurable value (page-size
  bounds), and nullable reference types + async/await throughout.
- A companion xUnit test project with both service-layer unit tests (EF Core InMemory
  provider) and full-stack integration tests (`WebApplicationFactory<Program>`, also on
  EF Core InMemory) — neither requires LocalDB or a real SQL Server to run. One
  integration test opens a real `HubConnection` (`Microsoft.AspNetCore.SignalR.Client`)
  against the test server and asserts a `PATCH /api/tasks/{id}/status` call actually
  broadcasts `TaskStatusChanged` to a connected client, not just that the hub route is
  mapped.

## Project layout

```
Demo1-AspNetCore-EFCore-API/
├── Demo1-AspNetCore-EFCore-API.sln
├── global.json                          # pins the .NET 8 SDK for this demo
├── src/TaskTracker.Api/
│   ├── Controllers/                     # ProjectsController, TasksController
│   ├── Data/
│   │   ├── TaskTrackerDbContext.cs
│   │   └── Migrations/                  # EF Core Code-First migration
│   ├── DTOs/                            # request/response DTOs, PagedResult<T>
│   ├── Hubs/                            # TaskHub + ITaskHubNotifier
│   ├── Models/                          # Project, TaskItem, TaskItemStatus
│   ├── Options/                         # PaginationOptions
│   ├── Services/                        # IProjectService/IProjectService, ITaskItemService
│   ├── wwwroot/signalr-test.html        # manual SignalR test client (see below)
│   ├── appsettings.json                 # LocalDB connection string placeholder, no secrets
│   └── Program.cs
└── tests/TaskTracker.Tests/
    ├── Services/                        # unit tests (EF Core InMemory)
    └── Integration/                     # WebApplicationFactory<Program> tests
```

## Prerequisites

- **.NET 8 SDK** (this demo pins `8.0.417` via `global.json`; any 8.0.x SDK works).
- **SQL Server LocalDB** (ships with Visual Studio) or any reachable SQL Server instance,
  for actually *running* the API. **Not required for running the tests** — those use the
  EF Core InMemory provider.
- The `dotnet-ef` global tool, only if you want to add/regenerate migrations yourself:
  `dotnet tool install --global dotnet-ef`.

## Configuration

`src/TaskTracker.Api/appsettings.json` contains a LocalDB connection string placeholder:

```json
"ConnectionStrings": {
  "TaskTrackerDb": "Server=(localdb)\\mssqllocaldb;Database=TaskTrackerDb;Trusted_Connection=True;"
}
```

To point at a different SQL Server instance, override `ConnectionStrings:TaskTrackerDb` in
an untracked `appsettings.Development.json`, a user-secret, or an environment variable
(`ConnectionStrings__TaskTrackerDb`) — do not edit the committed placeholder with real
credentials.

## Running the migrations

From `src/TaskTracker.Api/`:

```bash
dotnet ef database update
```

This creates the `TaskTrackerDb` database (on LocalDB by default) and applies the
`InitialCreate` migration, which creates the `Projects` and `Tasks` tables.

## Running the API

From `src/TaskTracker.Api/`:

```bash
dotnet run
```

By default this launches on the URLs in `Properties/launchSettings.json`. With the
`Development` environment (the default for `dotnet run`), Swagger UI is available at
`/swagger`.

Example requests (see also `TaskTracker.Api.http` for a ready-made set, or Swagger UI):

```bash
curl -X POST http://localhost:5248/api/projects \
  -H "Content-Type: application/json" \
  -d '{"name":"Website Revamp","description":"Redesign the marketing site"}'

curl -X POST http://localhost:5248/api/projects/1/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Design homepage mockup"}'

curl -X PATCH http://localhost:5248/api/tasks/1/status \
  -H "Content-Type: application/json" \
  -d '{"status":"InProgress"}'
```

Task status values are `Todo`, `InProgress`, `Done` (serialized as strings, not integers).

## Testing the SignalR hub

The hub is mapped at `/hubs/tasks`. Any status change made through
`PATCH /api/tasks/{id}/status` broadcasts a `TaskStatusChanged` event to every connected
client, with a payload of:

```json
{
  "taskId": 1,
  "projectId": 1,
  "title": "Design homepage mockup",
  "previousStatus": "Todo",
  "newStatus": "InProgress",
  "changedAtUtc": "2026-08-23T12:00:00Z"
}
```

**Option A — browser test client (easiest).** With the API running, open
`http://localhost:5248/signalr-test.html` in a browser. It connects to `/hubs/tasks` and
logs every `TaskStatusChanged` event it receives. Trigger one with the `PATCH` request
above (or via Swagger UI) while the page is open. This page loads the SignalR JS client
from a public CDN, so it needs internet access; it does not call any other external
service.

**Option B — confirm the hub is live without a browser.** SignalR negotiation is a plain
HTTP endpoint, so you can sanity-check it's mapped with curl:

```bash
curl -X POST "http://localhost:5248/hubs/tasks/negotiate?negotiateVersion=1"
```

A `200` with a JSON body listing `availableTransports` confirms the hub is reachable.
For a full duplex test without a browser, use a generic WebSocket test tool (e.g. a
WebSocket extension in Postman/Insomnia, or `wscat`) against
`ws://localhost:5248/hubs/tasks` following the
[SignalR client protocol handshake](https://learn.microsoft.com/aspnet/core/signalr/) —
the browser test client above is the more practical option for a quick check.

## Running the tests

From the demo root:

```bash
dotnet test tests/TaskTracker.Tests/TaskTracker.Tests.csproj
```

This runs both the service-layer unit tests and the `WebApplicationFactory<Program>`
integration tests. Both use the EF Core InMemory provider (via a
`CustomWebApplicationFactory` that swaps out the SQL Server `DbContext` registration for
the integration tests), so **no LocalDB or SQL Server instance is required** to run the
test suite.

## Building everything

```bash
dotnet build Demo1-AspNetCore-EFCore-API.sln
```
