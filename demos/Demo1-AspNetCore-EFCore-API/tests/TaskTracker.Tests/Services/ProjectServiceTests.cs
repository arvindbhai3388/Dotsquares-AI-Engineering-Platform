using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Options;
using TaskTracker.Api.Services;

namespace TaskTracker.Tests.Services;

public class ProjectServiceTests
{
    private static TaskTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TaskTrackerDbContext(options);
    }

    private static ProjectService CreateSut(TaskTrackerDbContext dbContext, PaginationOptions? paginationOptions = null)
    {
        return new ProjectService(dbContext, Options.Create(paginationOptions ?? new PaginationOptions()));
    }

    [Fact]
    public async Task CreateProjectAsync_PersistsProjectAndReturnsDto()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);

        var result = await sut.CreateProjectAsync(new ProjectCreateDto { Name = "Alpha", Description = "First project" });

        Assert.True(result.Id > 0);
        Assert.Equal("Alpha", result.Name);
        Assert.Equal(0, result.TaskCount);
        Assert.Equal(1, await dbContext.Projects.CountAsync());
    }

    [Fact]
    public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);

        var result = await sut.GetProjectByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProjectsAsync_ReturnsPagedResults_RespectingPageSize()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);

        for (var i = 1; i <= 5; i++)
        {
            await sut.CreateProjectAsync(new ProjectCreateDto { Name = $"Project {i}" });
        }

        var page1 = await sut.GetProjectsAsync(new PaginationQuery { PageNumber = 1, PageSize = 2 });

        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal("Project 1", page1.Items[0].Name);
    }

    [Fact]
    public async Task GetProjectsAsync_ClampsPageSize_ToMaxPageSize()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext, new PaginationOptions { DefaultPageSize = 10, MaxPageSize = 3 });

        for (var i = 1; i <= 5; i++)
        {
            await sut.CreateProjectAsync(new ProjectCreateDto { Name = $"Project {i}" });
        }

        var page1 = await sut.GetProjectsAsync(new PaginationQuery { PageNumber = 1, PageSize = 50 });

        Assert.Equal(3, page1.PageSize);
        Assert.Equal(3, page1.Items.Count);
    }

    [Fact]
    public async Task UpdateProjectAsync_ReturnsFalse_WhenProjectDoesNotExist()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);

        var updated = await sut.UpdateProjectAsync(123, new ProjectUpdateDto { Name = "Doesn't matter" });

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateProjectAsync_UpdatesFields_WhenProjectExists()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);
        var created = await sut.CreateProjectAsync(new ProjectCreateDto { Name = "Original" });

        var updated = await sut.UpdateProjectAsync(created.Id, new ProjectUpdateDto { Name = "Renamed", Description = "New description" });

        Assert.True(updated);
        var fetched = await sut.GetProjectByIdAsync(created.Id);
        Assert.Equal("Renamed", fetched!.Name);
        Assert.Equal("New description", fetched.Description);
    }

    [Fact]
    public async Task DeleteProjectAsync_ReturnsFalse_WhenProjectDoesNotExist()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);

        var deleted = await sut.DeleteProjectAsync(42);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteProjectAsync_RemovesProject_WhenItExists()
    {
        await using var dbContext = CreateContext();
        var sut = CreateSut(dbContext);
        var created = await sut.CreateProjectAsync(new ProjectCreateDto { Name = "To delete" });

        var deleted = await sut.DeleteProjectAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await sut.GetProjectByIdAsync(created.Id));
    }
}
