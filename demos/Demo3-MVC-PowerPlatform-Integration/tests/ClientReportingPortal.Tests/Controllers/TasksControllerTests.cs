using ClientReportingPortal.Web.Contracts.Tasks;
using ClientReportingPortal.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClientReportingPortal.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock = new();
    private readonly Mock<ILogger<TasksController>> _loggerMock = new();
    private readonly TasksController _sut;

    public TasksControllerTests()
    {
        _sut = new TasksController(_taskServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTasksFromService()
    {
        var tasks = new List<TaskItemDto>
        {
            new() { Id = 1, Title = "Task one" },
            new() { Id = 2, Title = "Task two" },
        };
        _taskServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        var result = await _sut.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(tasks, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenTaskExists()
    {
        var task = new TaskItemDto { Id = 5, Title = "Found" };
        _taskServiceMock.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.GetById(5, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(task, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTaskMissing()
    {
        _taskServiceMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItemDto?)null);

        var result = await _sut.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithLocationPointingAtGetById()
    {
        var request = new CreateTaskRequest { Title = "New task" };
        var created = new TaskItemDto { Id = 7, Title = "New task" };
        _taskServiceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _sut.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TasksController.GetById), createdResult.ActionName);
        Assert.Equal(7, ((RouteValueDictionary)createdResult.RouteValues!)["id"]);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_WhenModelStateInvalid()
    {
        _sut.ModelState.AddModelError("Title", "Title is required");
        var request = new CreateTaskRequest { Title = string.Empty };

        var result = await _sut.Create(request, CancellationToken.None);

        Assert.IsType<ObjectResult>(result.Result);
        _taskServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreateTaskRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenTaskExists()
    {
        var request = new UpdateTaskRequest { Title = "Updated" };
        var updated = new TaskItemDto { Id = 3, Title = "Updated" };
        _taskServiceMock.Setup(s => s.UpdateAsync(3, request, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var result = await _sut.Update(3, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(updated, okResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTaskMissing()
    {
        var request = new UpdateTaskRequest { Title = "Updated" };
        _taskServiceMock.Setup(s => s.UpdateAsync(404, request, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItemDto?)null);

        var result = await _sut.Update(404, request, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenTaskDeleted()
    {
        _taskServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTaskMissing()
    {
        _taskServiceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.Delete(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
