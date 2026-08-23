namespace ClientReportingPortal.Web.Contracts.Tasks;

/// <summary>
/// Backing store for the <c>/api/tasks</c> CRUD surface, kept behind an interface (like the
/// Power BI/SharePoint seams) so <c>Controllers/Api/TasksController</c> can be unit-tested with a
/// mock instead of a real database. The in-memory <see cref="Services.Tasks.InMemoryTaskService"/>
/// is the only implementation in this demo; a real deployment would back this with EF Core/SQL or
/// Dataverse (see the README's Power Apps custom-connector notes).
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TaskItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskItemDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns null when <paramref name="id"/> does not exist.</summary>
    Task<TaskItemDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns false when <paramref name="id"/> did not exist.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
