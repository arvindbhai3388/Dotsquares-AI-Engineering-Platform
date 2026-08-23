using TaskTracker.Api.DTOs;

namespace TaskTracker.Api.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectResponseDto>> GetProjectsAsync(PaginationQuery query, CancellationToken cancellationToken = default);

    Task<ProjectResponseDto?> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns false when no project with <paramref name="id"/> exists.</summary>
    Task<bool> UpdateProjectAsync(int id, ProjectUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns false when no project with <paramref name="id"/> exists.</summary>
    Task<bool> DeleteProjectAsync(int id, CancellationToken cancellationToken = default);
}
