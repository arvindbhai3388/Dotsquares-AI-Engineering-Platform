using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Hubs;
using TaskTracker.Api.Models;
using TaskTracker.Api.Options;
using TaskTracker.Api.Services;

namespace TaskTracker.Tests.Services;

public class TaskItemServiceTests
{
    private static TaskTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TaskTrackerDbContext(options);
    }

    private static TaskItemService CreateSut(
        TaskTrackerDbContext dbContext,
        Mock<ITaskHubNotifier> hubNotifierMock,
        PaginationOptions? paginationOptions = null)
    {
        return new TaskItemService(dbContext, hubNotifierMock.Object, Options.Create(paginationOptions ?? new PaginationOptions()));
    }

    private static async Task<Project> SeedProjectAsync(TaskTrackerDbContext dbContext, string name = "Seed Project")
    {
        var project = new Project { Name = name, CreatedAt = DateTime.UtcNow };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        return project;
    }

    [Fact]
    public async Task CreateTaskAsync_Throws_WhenProjectDoesNotExist()
    {
        await using var dbContext = CreateContext();
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CreateTaskAsync(999, new TaskItemCreateDto { Title = "Orphan task" }));
    }

    [Fact]
    public async Task CreateTaskAsync_PersistsTask_WithTodoStatus()
    {
        await using var dbContext = CreateContext();
        var project = await SeedProjectAsync(dbContext);
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);

        var result = await sut.CreateTaskAsync(project.Id, new TaskItemCreateDto { Title = "Write tests" });

        Assert.Equal(TaskItemStatus.Todo, result.Status);
        Assert.Equal(project.Id, result.ProjectId);
        hubNotifierMock.Verify(
            n => n.NotifyTaskStatusChangedAsync(It.IsAny<TaskStatusChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "creating a task should not itself raise a status-changed notification");
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        await using var dbContext = CreateContext();
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);

        var result = await sut.UpdateTaskStatusAsync(123, new TaskItemStatusUpdateDto { Status = TaskItemStatus.Done });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_BroadcastsNotification_WhenStatusChanges()
    {
        await using var dbContext = CreateContext();
        var project = await SeedProjectAsync(dbContext);
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);
        var task = await sut.CreateTaskAsync(project.Id, new TaskItemCreateDto { Title = "Ship feature" });

        var result = await sut.UpdateTaskStatusAsync(task.Id, new TaskItemStatusUpdateDto { Status = TaskItemStatus.InProgress });

        Assert.Equal(TaskItemStatus.InProgress, result!.Status);
        hubNotifierMock.Verify(
            n => n.NotifyTaskStatusChangedAsync(
                It.Is<TaskStatusChangedNotification>(notification =>
                    notification.TaskId == task.Id &&
                    notification.PreviousStatus == TaskItemStatus.Todo &&
                    notification.NewStatus == TaskItemStatus.InProgress),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_DoesNotBroadcast_WhenStatusIsUnchanged()
    {
        await using var dbContext = CreateContext();
        var project = await SeedProjectAsync(dbContext);
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);
        var task = await sut.CreateTaskAsync(project.Id, new TaskItemCreateDto { Title = "No-op" });

        await sut.UpdateTaskStatusAsync(task.Id, new TaskItemStatusUpdateDto { Status = TaskItemStatus.Todo });

        hubNotifierMock.Verify(
            n => n.NotifyTaskStatusChangedAsync(It.IsAny<TaskStatusChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTasksAsync_FiltersByProjectId()
    {
        await using var dbContext = CreateContext();
        var projectA = await SeedProjectAsync(dbContext, "A");
        var projectB = await SeedProjectAsync(dbContext, "B");
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);

        await sut.CreateTaskAsync(projectA.Id, new TaskItemCreateDto { Title = "A1" });
        await sut.CreateTaskAsync(projectA.Id, new TaskItemCreateDto { Title = "A2" });
        await sut.CreateTaskAsync(projectB.Id, new TaskItemCreateDto { Title = "B1" });

        var resultForA = await sut.GetTasksAsync(projectA.Id, new PaginationQuery());

        Assert.Equal(2, resultForA.TotalCount);
        Assert.All(resultForA.Items, t => Assert.Equal(projectA.Id, t.ProjectId));
    }

    [Fact]
    public async Task DeleteTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        await using var dbContext = CreateContext();
        var hubNotifierMock = new Mock<ITaskHubNotifier>();
        var sut = CreateSut(dbContext, hubNotifierMock);

        var deleted = await sut.DeleteTaskAsync(777);

        Assert.False(deleted);
    }
}
