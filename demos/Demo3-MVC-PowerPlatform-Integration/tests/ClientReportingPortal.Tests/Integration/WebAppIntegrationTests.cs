using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClientReportingPortal.Tests.Integration;

/// <summary>
/// End-to-end tests that boot the real app (all "Mock..." services included) via
/// <see cref="WebApplicationFactory{TEntryPoint}"/> and hit it over an in-memory HTTP client -
/// no external network access, no real Power BI/SharePoint tenant required.
/// </summary>
public class WebAppIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebAppIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Dashboard")]
    [InlineData("/Documents")]
    [InlineData("/Admin")]
    public async Task Page_ReturnsSuccess(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TasksApi_GetAll_ReturnsSeededTasks()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"title\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SwaggerJson_IsServed()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
