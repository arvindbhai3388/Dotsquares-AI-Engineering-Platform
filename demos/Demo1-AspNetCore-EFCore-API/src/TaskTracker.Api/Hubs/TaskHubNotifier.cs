using Microsoft.AspNetCore.SignalR;

namespace TaskTracker.Api.Hubs;

/// <inheritdoc cref="ITaskHubNotifier"/>
public class TaskHubNotifier : ITaskHubNotifier
{
    private const string TaskStatusChangedEvent = "TaskStatusChanged";

    private readonly IHubContext<TaskHub> _hubContext;

    public TaskHubNotifier(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyTaskStatusChangedAsync(TaskStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(TaskStatusChangedEvent, notification, cancellationToken);
    }
}
