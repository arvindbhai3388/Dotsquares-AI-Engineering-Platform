using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Options;

namespace TaskTracker.Api.Services;

/// <inheritdoc cref="IProjectService"/>
public class ProjectService : IProjectService
{
    private readonly TaskTrackerDbContext _dbContext;
    private readonly PaginationOptions _paginationOptions;

    public ProjectService(TaskTrackerDbContext dbContext, IOptions<PaginationOptions> paginationOptions)
    {
        _dbContext = dbContext;
        _paginationOptions = paginationOptions.Value;
    }

    public async Task<PagedResult<ProjectResponseDto>> GetProjectsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Min(
            query.PageSize <= 0 ? _paginationOptions.DefaultPageSize : query.PageSize,
            _paginationOptions.MaxPageSize);
        var pageNumber = Math.Max(query.PageNumber, 1);

        var baseQuery = _dbContext.Projects
            .AsNoTracking()
            .OrderBy(p => p.Id);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToDto(p))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => ToDto(p))
            .FirstOrDefaultAsync(cancellationToken);

        return project;
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateDto dto, CancellationToken cancellationToken = default)
    {
        var project = new Models.Project
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            TaskCount = 0
        };
    }

    public async Task<bool> UpdateProjectAsync(int id, ProjectUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project is null)
        {
            return false;
        }

        project.Name = dto.Name;
        project.Description = dto.Description;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteProjectAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project is null)
        {
            return false;
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProjectResponseDto ToDto(Models.Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        CreatedAt = p.CreatedAt,
        TaskCount = p.Tasks.Count
    };
}
