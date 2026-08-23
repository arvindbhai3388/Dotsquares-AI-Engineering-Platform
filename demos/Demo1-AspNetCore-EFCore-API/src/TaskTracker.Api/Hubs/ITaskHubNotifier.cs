namespace TaskTracker.Api.Hubs;

/// <summary>
/// Abstraction over broadcasting task notifications to connected SignalR clients.
/// Kept separate from <see cref="TaskHub"/> so business-logic services can depend
/// on an interface rather than the concrete SignalR hub context, which keeps the
/// service layer unit-testable without a live SignalR connection.
/// </summary>
public interface ITaskHubNotifier
{
    Task NotifyTaskStatusChangedAsync(TaskStatusChangedNotification notification, CancellationToken cancellationToken = default);
}
