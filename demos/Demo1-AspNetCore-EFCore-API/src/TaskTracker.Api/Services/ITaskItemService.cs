using TaskTracker.Api.DTOs;

namespace TaskTracker.Api.Services;

public interface ITaskItemService
{
    /// <param name="projectId">Optional filter to a single project's tasks.</param>
    Task<PagedResult<TaskItemResponseDto>> GetTasksAsync(int? projectId, PaginationQuery query, CancellationToken cancellationToken = default);

    Task<TaskItemResponseDto?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Throws <see cref="NotFoundException"/> when <paramref name="projectId"/> does not exist.</summary>
    Task<TaskItemResponseDto> CreateTaskAsync(int projectId, TaskItemCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns null when no task with <paramref name="id"/> exists.</summary>
    Task<TaskItemResponseDto?> UpdateTaskAsync(int id, TaskItemUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a task's status and broadcasts a "TaskStatusChanged" SignalR notification
    /// when the status actually changes. Returns null when no task with <paramref name="id"/> exists.
    /// </summary>
    Task<TaskItemResponseDto?> UpdateTaskStatusAsync(int id, TaskItemStatusUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns false when no task with <paramref name="id"/> exists.</summary>
    Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
}
