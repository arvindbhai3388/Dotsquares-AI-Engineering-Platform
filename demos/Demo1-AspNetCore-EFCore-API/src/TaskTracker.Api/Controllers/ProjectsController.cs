using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>Gets a paged list of projects.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProjectResponseDto>>> GetProjects(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single project by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponseDto>> GetProject(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: $"No project exists with id {id}."));
        }

        return Ok(project);
    }

    /// <summary>Creates a new project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponseDto>> CreateProject(
        [FromBody] ProjectCreateDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _projectService.CreateProjectAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetProject), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing project.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectUpdateDto dto, CancellationToken cancellationToken)
    {
        var updated = await _projectService.UpdateProjectAsync(id, dto, cancellationToken);
        if (!updated)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: $"No project exists with id {id}."));
        }

        return NoContent();
    }

    /// <summary>Deletes a project and its tasks.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken cancellationToken)
    {
        var deleted = await _projectService.DeleteProjectAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: $"No project exists with id {id}."));
        }

        return NoContent();
    }
}
