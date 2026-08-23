using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Hubs;
using TaskTracker.Api.Models;

namespace TaskTracker.Tests.Integration;

public class TaskTrackerApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    // Mirrors the API's own JsonStringEnumConverter registration (Program.cs) so response
    // bodies containing TaskItemStatus (serialized as e.g. "Done") deserialize correctly here.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TaskTrackerApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProject_ThenGetById_RoundTrips()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/projects", new ProjectCreateDto
        {
            Name = "Integration Test Project",
            Description = "Created from an integration test"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.Equal("Integration Test Project", fetched!.Name);
    }

    [Fact]
    public async Task CreateProject_WithMissingName_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new ProjectCreateDto { Name = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ForUnknownProject_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/999999/tasks", new TaskItemCreateDto { Title = "Orphan" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ThenUpdateStatus_PersistsNewStatus()
    {
        var project = await CreateProjectAsync();

        var createTaskResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/tasks",
            new TaskItemCreateDto { Title = "Implement feature" });
        createTaskResponse.EnsureSuccessStatusCode();
        var task = await createTaskResponse.Content.ReadFromJsonAsync<TaskItemResponseDto>(JsonOptions);
        Assert.NotNull(task);
        Assert.Equal(TaskItemStatus.Todo, task!.Status);

        var statusResponse = await _client.PatchAsJsonAsync(
            $"/api/tasks/{task.Id}/status",
            new TaskItemStatusUpdateDto { Status = TaskItemStatus.Done });

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var updated = await statusResponse.Content.ReadFromJsonAsync<TaskItemResponseDto>(JsonOptions);
        Assert.Equal(TaskItemStatus.Done, updated!.Status);

        var getResponse = await _client.GetAsync($"/api/tasks/{task.Id}");
        var refetched = await getResponse.Content.ReadFromJsonAsync<TaskItemResponseDto>(JsonOptions);
        Assert.Equal(TaskItemStatus.Done, refetched!.Status);
    }

    [Fact]
    public async Task GetTasks_SupportsPagination()
    {
        var project = await CreateProjectAsync();
        for (var i = 1; i <= 3; i++)
        {
            await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new TaskItemCreateDto { Title = $"Task {i}" });
        }

        var response = await _client.GetAsync($"/api/projects/{project.Id}/tasks?pageNumber=1&pageSize=2");
        response.EnsureSuccessStatusCode();

        var page = await response.Content.ReadFromJsonAsync<PagedResult<TaskItemResponseDto>>(JsonOptions);
        Assert.NotNull(page);
        Assert.Equal(2, page!.Items.Count);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task TaskHub_NegotiateEndpoint_IsMapped()
    {
        var response = await _client.PostAsync("/hubs/tasks/negotiate?negotiateVersion=1", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTaskStatus_BroadcastsTaskStatusChangedToConnectedClients()
    {
        var project = await CreateProjectAsync();
        var createTaskResponse = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/tasks",
            new TaskItemCreateDto { Title = "Broadcast test task" });
        createTaskResponse.EnsureSuccessStatusCode();
        var task = await createTaskResponse.Content.ReadFromJsonAsync<TaskItemResponseDto>(JsonOptions);
        Assert.NotNull(task);

        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_client.BaseAddress!, "/hubs/tasks"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        var notificationReceived = new TaskCompletionSource<TaskStatusChangedNotification>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<TaskStatusChangedNotification>(
            "TaskStatusChanged",
            notification => notificationReceived.TrySetResult(notification));

        await connection.StartAsync();

        var statusResponse = await _client.PatchAsJsonAsync(
            $"/api/tasks/{task!.Id}/status",
            new TaskItemStatusUpdateDto { Status = TaskItemStatus.InProgress });
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var completedTask = await Task.WhenAny(notificationReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(notificationReceived.Task, completedTask);

        var notification = await notificationReceived.Task;
        Assert.Equal(task.Id, notification.TaskId);
        Assert.Equal(project.Id, notification.ProjectId);
        Assert.Equal(task.Title, notification.Title);
        Assert.Equal(TaskItemStatus.Todo, notification.PreviousStatus);
        Assert.Equal(TaskItemStatus.InProgress, notification.NewStatus);
    }

    private async Task<ProjectResponseDto> CreateProjectAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new ProjectCreateDto { Name = $"Project {Guid.NewGuid()}" });
        response.EnsureSuccessStatusCode();
        var project = await response.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);
        return project!;
    }
}
