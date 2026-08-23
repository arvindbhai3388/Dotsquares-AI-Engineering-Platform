using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskItemService _taskItemService;

    public TasksController(ITaskItemService taskItemService)
    {
        _taskItemService = taskItemService;
    }

    /// <summary>Gets a paged list of tasks, optionally scoped to a single project.</summary>
    [HttpGet("api/projects/{projectId:int}/tasks")]
    [ProducesResponseType(typeof(PagedResult<TaskItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TaskItemResponseDto>>> GetTasksForProject(
        int projectId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _taskItemService.GetTasksAsync(projectId, query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a paged list across all tasks.</summary>
    [HttpGet("api/tasks")]
    [ProducesResponseType(typeof(PagedResult<TaskItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TaskItemResponseDto>>> GetAllTasks(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _taskItemService.GetTasksAsync(projectId: null, query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single task by id.</summary>
    [HttpGet("api/tasks/{id:int}", Name = nameof(GetTaskById))]
    [ProducesResponseType(typeof(TaskItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemResponseDto>> GetTaskById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskItemService.GetTaskByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound(TaskNotFoundProblem(id));
        }

        return Ok(task);
    }

    /// <summary>Creates a new task under the given project.</summary>
    [HttpPost("api/projects/{projectId:int}/tasks")]
    [ProducesResponseType(typeof(TaskItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskItemResponseDto>> CreateTask(
        int projectId,
        [FromBody] TaskItemCreateDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _taskItemService.CreateTaskAsync(projectId, dto, cancellationToken);
            return CreatedAtRoute(nameof(GetTaskById), new { id = created.Id }, created);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status404NotFound,
                title: "Project not found",
                detail: ex.Message));
        }
    }

    /// <summary>Updates a task's title, description, and due date.</summary>
    [HttpPut("api/tasks/{id:int}")]
    [ProducesResponseType(typeof(TaskItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemResponseDto>> UpdateTask(int id, [FromBody] TaskItemUpdateDto dto, CancellationToken cancellationToken)
    {
        var updated = await _taskItemService.UpdateTaskAsync(id, dto, cancellationToken);
        if (updated is null)
        {
            return NotFound(TaskNotFoundProblem(id));
        }

        return Ok(updated);
    }

    /// <summary>
    /// Updates a task's status. Broadcasts a "TaskStatusChanged" event over the
    /// <c>/hubs/tasks</c> SignalR hub when the status actually changes.
    /// </summary>
    [HttpPatch("api/tasks/{id:int}/status")]
    [ProducesResponseType(typeof(TaskItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemResponseDto>> UpdateTaskStatus(int id, [FromBody] TaskItemStatusUpdateDto dto, CancellationToken cancellationToken)
    {
        var updated = await _taskItemService.UpdateTaskStatusAsync(id, dto, cancellationToken);
        if (updated is null)
        {
            return NotFound(TaskNotFoundProblem(id));
        }

        return Ok(updated);
    }

    /// <summary>Deletes a task.</summary>
    [HttpDelete("api/tasks/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken)
    {
        var deleted = await _taskItemService.DeleteTaskAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(TaskNotFoundProblem(id));
        }

        return NoContent();
    }

    private ProblemDetails TaskNotFoundProblem(int id) => ProblemDetailsFactory.CreateProblemDetails(
        HttpContext,
        statusCode: StatusCodes.Status404NotFound,
        title: "Task not found",
        detail: $"No task exists with id {id}.");
}
