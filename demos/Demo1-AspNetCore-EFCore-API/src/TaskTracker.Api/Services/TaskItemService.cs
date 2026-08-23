using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Hubs;
using TaskTracker.Api.Options;

namespace TaskTracker.Api.Services;

/// <inheritdoc cref="ITaskItemService"/>
public class TaskItemService : ITaskItemService
{
    private readonly TaskTrackerDbContext _dbContext;
    private readonly ITaskHubNotifier _hubNotifier;
    private readonly PaginationOptions _paginationOptions;

    public TaskItemService(
        TaskTrackerDbContext dbContext,
        ITaskHubNotifier hubNotifier,
        IOptions<PaginationOptions> paginationOptions)
    {
        _dbContext = dbContext;
        _hubNotifier = hubNotifier;
        _paginationOptions = paginationOptions.Value;
    }

    public async Task<PagedResult<TaskItemResponseDto>> GetTasksAsync(int? projectId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Min(
            query.PageSize <= 0 ? _paginationOptions.DefaultPageSize : query.PageSize,
            _paginationOptions.MaxPageSize);
        var pageNumber = Math.Max(query.PageNumber, 1);

        var baseQuery = _dbContext.Tasks.AsNoTracking().AsQueryable();
        if (projectId.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.ProjectId == projectId.Value);
        }

        baseQuery = baseQuery.OrderBy(t => t.Id);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToDto(t))
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItemResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskItemResponseDto?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => ToDto(t))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TaskItemResponseDto> CreateTaskAsync(int projectId, TaskItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            throw new NotFoundException($"Project {projectId} was not found.");
        }

        var now = DateTime.UtcNow;
        var task = new Models.TaskItem
        {
            ProjectId = projectId,
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Status = Models.TaskItemStatus.Todo,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(task);
    }

    public async Task<TaskItemResponseDto?> UpdateTaskAsync(int id, TaskItemUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(task);
    }

    public async Task<TaskItemResponseDto?> UpdateTaskStatusAsync(int id, TaskItemStatusUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var previousStatus = task.Status;
        if (previousStatus == dto.Status)
        {
            return ToDto(task);
        }

        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _hubNotifier.NotifyTaskStatusChangedAsync(
            new TaskStatusChangedNotification(
                task.Id,
                task.ProjectId,
                task.Title,
                previousStatus,
                task.Status,
                task.UpdatedAt),
            cancellationToken);

        return ToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static TaskItemResponseDto ToDto(Models.TaskItem t) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
