using System.Collections.Concurrent;
using ClientReportingPortal.Web.Contracts.Tasks;

namespace ClientReportingPortal.Web.Services.Tasks;

/// <summary>
/// Thread-safe, in-memory <see cref="ITaskService"/> used to back the demo's
/// Power-Apps-custom-connector-shaped <c>/api/tasks</c> controller. No database required to run
/// this demo; swap for an EF Core/SQL- or Dataverse-backed implementation for production use.
/// </summary>
public sealed class InMemoryTaskService : ITaskService
{
    private readonly ConcurrentDictionary<int, TaskItemDto> _tasks = new();
    private int _nextId;

    public InMemoryTaskService()
    {
        Seed("Prepare Q3 client report", "Pull latest figures from the Power BI dataset and validate totals.", "Priya Sharma", DateTimeOffset.UtcNow.AddDays(3));
        Seed("Renew SharePoint app registration secret", "Client secret for the Graph app registration expires soon.", "Arvind Kushwaha", DateTimeOffset.UtcNow.AddDays(10));
        Seed("Review Power Apps connector permissions", "Confirm the custom connector still uses least-privilege scopes.", null, null);
    }

    private void Seed(string title, string? description, string? assignedTo, DateTimeOffset? dueDateUtc)
    {
        var id = Interlocked.Increment(ref _nextId);
        _tasks[id] = new TaskItemDto
        {
            Id = id,
            Title = title,
            Description = description,
            IsCompleted = false,
            DueDateUtc = dueDateUtc,
            AssignedTo = assignedTo,
        };
    }

    public Task<IReadOnlyList<TaskItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TaskItemDto> all = _tasks.Values.OrderBy(t => t.Id).ToList();
        return Task.FromResult(all);
    }

    public Task<TaskItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task<TaskItemDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = Interlocked.Increment(ref _nextId);
        var task = new TaskItemDto
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            DueDateUtc = request.DueDateUtc,
            AssignedTo = request.AssignedTo,
        };

        _tasks[id] = task;
        return Task.FromResult(task);
    }

    public Task<TaskItemDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_tasks.ContainsKey(id))
        {
            return Task.FromResult<TaskItemDto?>(null);
        }

        var updated = new TaskItemDto
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted,
            DueDateUtc = request.DueDateUtc,
            AssignedTo = request.AssignedTo,
        };

        _tasks[id] = updated;
        return Task.FromResult<TaskItemDto?>(updated);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tasks.TryRemove(id, out _));
    }
}
