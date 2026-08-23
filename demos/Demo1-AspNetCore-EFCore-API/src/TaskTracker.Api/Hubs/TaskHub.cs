using Microsoft.AspNetCore.SignalR;

namespace TaskTracker.Api.Hubs;

/// <summary>
/// SignalR hub that pushes real-time task notifications to connected clients.
/// Clients connect (no server-invocable methods are required for this demo)
/// and listen for the "TaskStatusChanged" event broadcast by
/// <see cref="ITaskHubNotifier"/> whenever a task's status changes via the REST API.
/// </summary>
public class TaskHub : Hub
{
}
