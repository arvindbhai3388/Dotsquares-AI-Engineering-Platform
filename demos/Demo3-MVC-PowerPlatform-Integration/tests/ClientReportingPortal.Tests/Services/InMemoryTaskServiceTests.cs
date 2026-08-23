using ClientReportingPortal.Web.Contracts.Tasks;
using ClientReportingPortal.Web.Services.Tasks;

namespace ClientReportingPortal.Tests.Services;

public class InMemoryTaskServiceTests
{
    private readonly InMemoryTaskService _sut = new();

    [Fact]
    public async Task GetAllAsync_ReturnsSeededTasks()
    {
        var tasks = await _sut.GetAllAsync();

        Assert.NotEmpty(tasks);
    }

    [Fact]
    public async Task CreateAsync_AddsTaskRetrievableById()
    {
        var request = new CreateTaskRequest { Title = "New task", Description = "desc", AssignedTo = "bob" };

        var created = await _sut.CreateAsync(request);
        var fetched = await _sut.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal("New task", fetched!.Title);
        Assert.False(fetched.IsCompleted);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForUnknownId()
    {
        var result = await _sut.GetByIdAsync(-1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingTask()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "Original" });

        var updated = await _sut.UpdateAsync(created.Id, new UpdateTaskRequest { Title = "Updated", IsCompleted = true });

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Title);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullForUnknownId()
    {
        var result = await _sut.UpdateAsync(-1, new UpdateTaskRequest { Title = "Does not exist" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingTaskAndReturnsTrue()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "Temp" });

        var deleted = await _sut.DeleteAsync(created.Id);
        var afterDelete = await _sut.GetByIdAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalseForUnknownId()
    {
        var deleted = await _sut.DeleteAsync(-1);

        Assert.False(deleted);
    }
}
