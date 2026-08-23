using TaskTracker.Api.Models;

namespace TaskTracker.Api.Hubs;

/// <summary>
/// Payload broadcast to SignalR clients on the "TaskStatusChanged" event.
/// </summary>
public record TaskStatusChangedNotification(
    int TaskId,
    int ProjectId,
    string Title,
    TaskItemStatus PreviousStatus,
    TaskItemStatus NewStatus,
    DateTime ChangedAtUtc);
